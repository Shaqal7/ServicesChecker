# Plan: Auto-Update Mechanism for ServicesChecker

## Overview

Add auto-update functionality that checks GitHub Releases for new versions, notifies the user, downloads the update, and applies it via a batch script that replaces the executable after the app closes.

**GitHub Repo**: `Shaqal7/ServicesChecker`
**Release Asset**: `ServicesChecker-win-x64.zip` (self-contained single-file exe)
**Version Format**: `v{YYYY.MM.dd}-{SHA7}` (e.g. `v2026.02.06-dc0cb80`)

---

## Phase 1: Version Embedding (Build Pipeline)

### 1.1 Modify workflow to embed version into assembly

**File**: [release.yml](../.github/workflows/release.yml)

Move the "Generate version tag" step **before** "Publish application" and pass it as `InformationalVersion`:

```yaml
- name: Generate version tag
  id: version
  run: |
    $date = Get-Date -Format "yyyy.MM.dd"
    $sha = "${{ github.sha }}".Substring(0, 7)
    $tag = "v$date-$sha"
    echo "tag=$tag" >> $env:GITHUB_OUTPUT
  shell: pwsh

- name: Publish application
  run: >
    dotnet publish ${{ env.PROJECT_PATH }}
    --configuration Release
    --runtime win-x64
    --self-contained true
    -p:PublishSingleFile=true
    -p:PublishReadyToRun=true
    -p:PublishTrimmed=false
    -p:InformationalVersion=${{ steps.version.outputs.tag }}
    --output ${{ env.PUBLISH_DIR }}
```

### 1.2 Add default dev version for local builds

**File**: [Directory.Build.props](../Directory.Build.props)

```xml
<InformationalVersion Condition="'$(InformationalVersion)' == ''">dev</InformationalVersion>
```

**Runtime access**:
```csharp
Assembly.GetEntryAssembly()?
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
    .InformationalVersion ?? "dev";
```

---

## Phase 2: Domain Layer

### 2.1 New entity: `UpdateInfo`

**New file**: `src/ServicesChecker.Domain/Entities/UpdateInfo.cs`

Properties:
- `TagName` (string) - version tag e.g. `v2026.02.06-dc0cb80`
- `ReleaseName` (string) - human-readable name
- `HtmlUrl` (string) - link to release page
- `DownloadUrl` (string) - direct ZIP download URL
- `AssetSizeBytes` (long) - for progress calculation
- `PublishedAt` (DateTime)
- `IsNewerThanCurrent` (bool)

---

## Phase 3: Application Layer

### 3.1 New interface: `IUpdateService`

**New file**: `src/ServicesChecker.Application/Interfaces/Services/IUpdateService.cs`

```csharp
public interface IUpdateService
{
    string GetCurrentVersion();
    Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default);
    Task<string> DownloadUpdateAsync(UpdateInfo updateInfo, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    void ApplyUpdateAndRestart(string stagingPath);
}
```

---

## Phase 4: Infrastructure Layer

### 4.1 New service: `GitHubUpdateService`

**New file**: `src/ServicesChecker.Infrastructure/Services/GitHubUpdateService.cs`

#### CheckForUpdateAsync
1. GET `https://api.github.com/repos/Shaqal7/ServicesChecker/releases/latest`
2. Parse JSON response (private DTOs with `[JsonPropertyName]`)
3. Find asset named `ServicesChecker-win-x64.zip`
4. **Normalize both versions** (current and release) by truncating SHA to 7 characters
5. Compare normalized versions (if current is `"dev"` → return null)
6. Return `UpdateInfo` if newer, `null` otherwise
7. All exceptions caught → return `null` (silent failure)

**Version Normalization** (added to fix hash length mismatch):
- Handles both short (7-char) and long (40-char) SHA formats
- Input: `v2026.02.05-7cd4d77` or `v2026.02.05-7cd4d778a82e1e3cfcf2b3b3aed9dcb564e8a2c4`
- Output: `v2026.02.05-7cd4d77` (always 7-char SHA for consistent comparison)

#### DownloadUpdateAsync
1. Create staging dir: `{AppDir}\_update_staging\`
2. Stream-download ZIP with progress reporting
3. Extract to `{staging}\extracted\`
4. Delete ZIP, return extracted path

#### ApplyUpdateAndRestart
1. Write `_update.cmd` batch script to staging dir
2. Launch script detached via `Process.Start`
3. Shutdown app via `Environment.Exit(0)`

#### Batch script logic (`_update.cmd`):
```batch
@echo off
:wait
tasklist /FI "PID eq {PID}" | find "{PID}" >NUL && (timeout /t 1 >NUL & goto wait)
robocopy "{STAGING}" "{APP_DIR}" /E /XF settings.json services.json logfiles.json /NFL /NDL /NJH /NJS /NC /NS /NP >NUL
start "" "{APP_DIR}\ServicesChecker.exe"
rmdir /S /Q "{STAGING_ROOT}" >NUL 2>NUL
del "%~f0" >NUL 2>NUL
```

Key: `robocopy` with `/XF` excludes JSON data files from being overwritten.

### 4.2 DI registration

**File**: [InfrastructureServiceExtensions.cs](../src/ServicesChecker.Infrastructure/DependencyInjection/InfrastructureServiceExtensions.cs)

```csharp
services.AddHttpClient<IUpdateService, GitHubUpdateService>(client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("User-Agent", "ServicesChecker");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

### 4.3 Cleanup on startup

On initialization, delete any leftover `_update_staging` directory from previous runs.

---

## Phase 5: UI Layer — ViewModel

**File**: [MainWindowViewModel.cs](../src/ServicesChecker.UI/ViewModels/MainWindowViewModel.cs)

### New dependency
- `IUpdateService _updateService` (constructor injection)

### New properties
```csharp
[ObservableProperty] private bool _isUpdateAvailable;
[ObservableProperty] private UpdateInfo? _availableUpdate;
[ObservableProperty] private bool _isDownloadingUpdate;
[ObservableProperty] private double _downloadProgress;
[ObservableProperty] private string? _updateStatusMessage;
public string CurrentVersion => _updateService.GetCurrentVersion();
```

### New commands
- `CheckForUpdateCommand` — calls `CheckForUpdateAsync`, sets `IsUpdateAvailable` if newer found
- `DownloadAndApplyUpdateCommand` — downloads with progress, then calls `ApplyUpdateAndRestart`

### Startup auto-check
```csharp
_ = CheckForUpdateAsync();  // fire-and-forget, non-blocking
```

---

## Phase 6: UI Layer — AXAML

**File**: [MainWindow.axaml](../src/ServicesChecker.UI/Views/MainWindow.axaml)

### Title bar changes
Change grid from `ColumnDefinitions="*,Auto"` to `ColumnDefinitions="*,Auto,Auto"`:

- **Column 1**: Update button (visible when `IsUpdateAvailable && !IsDownloadingUpdate`)
  - Download icon + "Update" text, AccentBrush color
  - Tooltip shows version info
  - Command: `DownloadAndApplyUpdateCommand`

- **Column 1 (alternate)**: Progress indicator (visible when `IsDownloadingUpdate`)
  - `ProgressBar` (0→1) + status text

- **Column 2**: Existing theme toggle button (moved from column 1)

### Optional: Version display
Small version text near the app title (opacity 0.5).

---

## Phase 7: Tests

### Domain tests
**New file**: `tests/ServicesChecker.Domain.Tests/Entities/UpdateInfoTests.cs`
- Default values, property setters

### Infrastructure tests
**New file**: `tests/ServicesChecker.Infrastructure.Tests/Services/GitHubUpdateServiceTests.cs`
- `CheckForUpdateAsync` — newer version returns `UpdateInfo`
- `CheckForUpdateAsync` — same version returns `null`
- `CheckForUpdateAsync` — dev version returns `null`
- `CheckForUpdateAsync` — network error returns `null`
- `CheckForUpdateAsync` — invalid JSON returns `null`
- `CheckForUpdateAsync` — no matching asset returns `null`
- `GetCurrentVersion` — returns non-null string
- Mock `HttpClient` via custom `HttpMessageHandler`

### ViewModel tests
**File**: `tests/ServicesChecker.UI.Tests/ViewModels/MainWindowViewModelTests.cs` (extend existing)
- `CheckForUpdateCommand` sets `IsUpdateAvailable` when update found
- `CheckForUpdateCommand` silent on error
- `CurrentVersion` delegates to service

---

## Files Summary

### New files (4):
| File | Layer |
|------|-------|
| `src/ServicesChecker.Domain/Entities/UpdateInfo.cs` | Domain |
| `src/ServicesChecker.Application/Interfaces/Services/IUpdateService.cs` | Application |
| `src/ServicesChecker.Infrastructure/Services/GitHubUpdateService.cs` | Infrastructure |
| `tests/ServicesChecker.Infrastructure.Tests/Services/GitHubUpdateServiceTests.cs` | Tests |

### Modified files (6):
| File | Change |
|------|--------|
| `Directory.Build.props` | Add default `InformationalVersion` |
| `.github/workflows/release.yml` | Reorder steps, add `-p:InformationalVersion` |
| `src/ServicesChecker.Infrastructure/DependencyInjection/InfrastructureServiceExtensions.cs` | Register `IUpdateService` |
| `src/ServicesChecker.UI/ViewModels/MainWindowViewModel.cs` | Add update properties & commands |
| `src/ServicesChecker.UI/Views/MainWindow.axaml` | Add update button to title bar |
| `tests/ServicesChecker.Domain.Tests/Entities/UpdateInfoTests.cs` | New entity tests |

---

## Verification

1. **Build**: `dotnet build ServicesChecker.sln`
2. **Tests**: `dotnet test ServicesChecker.sln` — all existing + new tests pass
3. **Manual test (dev)**: Run locally → version shows "dev" → no update notification (correct)
4. **Manual test (release)**: After CI builds with version tag → app detects update → download → batch script replaces exe → app restarts with new version
