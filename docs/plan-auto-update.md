# Plan: Auto-Update Mechanism for ServicesChecker

## Overview

Add auto-update functionality that checks GitHub Releases for new versions, notifies the user, downloads the update, and applies it via a batch script that replaces the executable after the app closes.

**GitHub Repo**: `Shaqal7/ServicesChecker`
**Release Asset**: `ServicesChecker-win-x64.zip` (self-contained single-file exe)
**Version Format**: `v{YYYY.MM.dd}-{SHA7}` (e.g. `v2026.02.06-dc0cb80`)

---

## Phase 1: Version Embedding (Build Pipeline)

### 1.1 Modify workflow to embed version into assembly

**File**: [release.yml](.github/workflows/release.yml)

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

**File**: [Directory.Build.props](Directory.Build.props)

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
4. Compare `tag_name` with current version (if current is `"dev"` → return null)
5. Return `UpdateInfo` if newer, `null` otherwise
6. All exceptions caught → return `null` (silent failure)

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

**File**: [InfrastructureServiceExtensions.cs](src/ServicesChecker.Infrastructure/DependencyInjection/InfrastructureServiceExtensions.cs)

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

**File**: [MainWindowViewModel.cs](src/ServicesChecker.UI/ViewModels/MainWindowViewModel.cs)

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

**File**: [MainWindow.axaml](src/ServicesChecker.UI/Views/MainWindow.axaml)

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

---

## Implementation Status

✅ **COMPLETED** - All phases implemented and tested successfully (2026-02-06)

### Bug Fixes Applied:
1. **Version normalization** - Added `NormalizeVersion()` method to handle SHA length mismatch (full 40-char vs 7-char)
2. **Batch script path escaping** - Fixed trailing backslash issue causing robocopy failure by using `TrimEnd('\\')`
3. **Update logging** - Added comprehensive logging to `_update.log` for debugging

### Verification Results:
- ✅ Version detection works correctly
- ✅ Update button shows only when newer version available
- ✅ Download with progress works
- ✅ Batch script successfully copies files (robocopy exit code 3 = success)
- ✅ Application restarts with new version
- ✅ Protected files (settings.json, services.json, logfiles.json, _update.log) preserved

---

## Future Enhancements (Optional)

### Enhancement: Update Installation Progress Feedback

**Problem**: After download completes, user sees no feedback during file installation (robocopy phase). Application closes and reopens with no indication of what's happening in between.

**Current Flow**:
1. User clicks "Update" ✅
2. Download progress bar shows (0-100%) ✅
3. Download completes
4. Application closes ⚠️ (no feedback)
5. Batch script waits for app to close
6. Robocopy copies files (2-3 seconds) ⚠️ (no feedback)
7. Application reopens with new version

**Proposed Options**:

#### Option 1: Status Message with Indefinite Progress (Recommended - Simple)
**Complexity**: Low
**Implementation Time**: 1-2 hours

**Changes**:
- **ViewModel** (`MainWindowViewModel.cs`):
  - After download completes, set `UpdateStatusMessage = "Installing update..."`
  - Show indefinite progress indicator (IsIndeterminate=true)
  - Add `await Task.Delay(2000)` before calling `ApplyUpdateAndRestart()`
  - User sees "Installing update..." for 2 seconds before app closes

- **UI** (`MainWindow.axaml`):
  - Modify progress indicator to support IsIndeterminate mode
  - Bind status text to `UpdateStatusMessage`

**Pros**:
- Simple to implement
- Provides clear feedback without complexity
- No parsing of external process output needed

**Cons**:
- No actual progress percentage (just spinner)
- User doesn't know exactly how long it will take

---

#### Option 2: Visible Console Window
**Complexity**: Very Low
**Implementation Time**: 15 minutes

**Changes**:
- **Infrastructure** (`GitHubUpdateService.cs`):
  - Change `CreateNoWindow = true` to `CreateNoWindow = false` in `Process.Start`
  - User sees console window with robocopy output (includes percentages)

**Pros**:
- Immediate feedback with real progress percentages
- Zero code changes to progress tracking
- Easy debugging

**Cons**:
- Less professional appearance (black console window)
- Window may appear behind main window
- Inconsistent with modern UI expectations

---

#### Option 3: Advanced Progress Window (Complex)
**Complexity**: High
**Implementation Time**: 8-12 hours

**Architecture**:
1. Create separate **UpdateHelper.exe** process
2. Launch helper before closing main app
3. Helper shows modern progress window
4. Helper monitors `_update.log` file in real-time
5. Parses robocopy output for percentage
6. Updates progress bar
7. Exits when update completes

**New Files**:
- `src/ServicesChecker.UpdateHelper/` (new console project)
- `src/ServicesChecker.UpdateHelper/ProgressWindow.axaml` (Avalonia window)
- `src/ServicesChecker.UpdateHelper/LogParser.cs`
- Modify build to include UpdateHelper.exe in release

**Pros**:
- Professional, polished user experience
- Real progress percentage from robocopy
- Separate process survives main app shutdown

**Cons**:
- Significantly more complex
- Requires additional project and build configuration
- Log file parsing can be fragile
- Overkill for 2-3 second operation

---

### Recommendation

**For now: Option 1 (Status Message with Indefinite Progress)**

Rationale:
- Robocopy typically completes in 2-3 seconds (verified in logs)
- Simple spinner + "Installing update..." provides sufficient feedback
- Low implementation cost vs. user experience improvement
- Can upgrade to Option 3 later if installation time increases

**Implementation Plan for Option 1** (when ready):

1. **ViewModel Changes** (`MainWindowViewModel.cs`):
   ```csharp
   [RelayCommand]
   private async Task DownloadAndApplyUpdateAsync()
   {
       try
       {
           IsDownloadingUpdate = true;
           UpdateStatusMessage = "Downloading update...";

           var stagingPath = await _updateService.DownloadUpdateAsync(
               AvailableUpdate!,
               new Progress<double>(p => DownloadProgress = p));

           // NEW: Show installing status
           UpdateStatusMessage = "Installing update...";
           DownloadProgress = 0; // Reset to show indeterminate
           await Task.Delay(2000); // Let user see the message

           _updateService.ApplyUpdateAndRestart(stagingPath);
       }
       catch (Exception ex)
       {
           SetError($"Update failed: {ex.Message}");
           IsDownloadingUpdate = false;
       }
   }
   ```

2. **UI Changes** (`MainWindow.axaml`):
   ```xml
   <ProgressBar IsVisible="{Binding IsDownloadingUpdate}"
                Value="{Binding DownloadProgress}"
                IsIndeterminate="{Binding DownloadProgress, Converter={StaticResource IsZeroConverter}}"
                Minimum="0" Maximum="1" />
   <TextBlock Text="{Binding UpdateStatusMessage}" />
   ```

3. **New Converter** (`IsZeroConverter.cs`):
   ```csharp
   // Returns true if value is 0 (for indeterminate mode)
   public class IsZeroConverter : IValueConverter
   {
       public object Convert(object value, ...) => (double)value == 0.0;
   }
   ```

4. **Tests**:
   - Update `MainWindowViewModelTests.cs` to verify status message changes
   - Verify indeterminate mode activates when progress = 0

**Deferred**: Can implement later if needed
