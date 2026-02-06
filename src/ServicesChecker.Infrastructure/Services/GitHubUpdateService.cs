using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Infrastructure.Services;

public class GitHubUpdateService : IUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly IFileSystemService _fileSystemService;

    private const string GitHubApiUrl =
        "https://api.github.com/repos/Shaqal7/ServicesChecker/releases/latest";

    private const string ExpectedAssetName = "ServicesChecker-win-x64.zip";
    private const string StagingDirName = "_update_staging";

    private static readonly HashSet<string> ProtectedFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "settings.json",
        "services.json",
        "logfiles.json"
    };

    public GitHubUpdateService(HttpClient httpClient, IFileSystemService fileSystemService)
    {
        _httpClient = httpClient;
        _fileSystemService = fileSystemService;

        CleanupLeftoverStaging();
    }

    public string GetCurrentVersion()
    {
        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "dev";

        return NormalizeVersion(version);
    }

    public async Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currentVersion = GetCurrentVersion();
            if (currentVersion == "dev")
                return null;

            var response = await _httpClient.GetAsync(GitHubApiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var release = JsonSerializer.Deserialize<GitHubRelease>(json);

            if (release is null || string.IsNullOrEmpty(release.TagName))
                return null;

            var releaseVersion = NormalizeVersion(release.TagName);
            if (releaseVersion == currentVersion)
                return null;

            var asset = release.Assets.FirstOrDefault(
                a => a.Name.Equals(ExpectedAssetName, StringComparison.OrdinalIgnoreCase));

            if (asset is null)
                return null;

            return new UpdateInfo
            {
                TagName = release.TagName,
                ReleaseName = release.Name,
                HtmlUrl = release.HtmlUrl,
                DownloadUrl = asset.BrowserDownloadUrl,
                AssetSizeBytes = asset.Size,
                PublishedAt = release.PublishedAt,
                IsNewerThanCurrent = true
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<string> DownloadUpdateAsync(
        UpdateInfo updateInfo,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var appDir = _fileSystemService.GetAppDirectory();
        var stagingDir = Path.Combine(appDir, StagingDirName);
        var extractedDir = Path.Combine(stagingDir, "extracted");
        var zipPath = Path.Combine(stagingDir, ExpectedAssetName);

        if (Directory.Exists(stagingDir))
            Directory.Delete(stagingDir, true);

        Directory.CreateDirectory(stagingDir);
        Directory.CreateDirectory(extractedDir);

        using var response = await _httpClient.GetAsync(
            updateInfo.DownloadUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? updateInfo.AssetSizeBytes;

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        long bytesRead = 0;
        int read;

        while ((read = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            bytesRead += read;

            if (totalBytes > 0)
                progress?.Report((double)bytesRead / totalBytes);
        }

        fileStream.Close();

        ZipFile.ExtractToDirectory(zipPath, extractedDir, true);
        File.Delete(zipPath);

        return extractedDir;
    }

    public void ApplyUpdateAndRestart(string stagingPath)
    {
        var appDir = _fileSystemService.GetAppDirectory();
        var currentPid = Environment.ProcessId;
        var stagingRoot = Path.Combine(appDir, StagingDirName);
        var scriptPath = Path.Combine(stagingRoot, "_update.cmd");

        var protectedFilesExclusion = string.Join(" ",
            ProtectedFiles.Select(f => $"\"{f}\""));

        var script = $"""
            @echo off
            :wait
            tasklist /FI "PID eq {currentPid}" 2>NUL | find /I "{currentPid}" >NUL
            if %ERRORLEVEL%==0 (
                timeout /t 1 /nobreak >NUL
                goto wait
            )
            robocopy "{stagingPath}" "{appDir}" /E /XF {protectedFilesExclusion} /NFL /NDL /NJH /NJS /NC /NS /NP >NUL
            start "" "{Path.Combine(appDir, "ServicesChecker.exe")}"
            rmdir /S /Q "{stagingRoot}" >NUL 2>NUL
            del "%~f0" >NUL 2>NUL
            """;

        File.WriteAllText(scriptPath, script);

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C \"{scriptPath}\"",
            CreateNoWindow = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden
        });

        Environment.Exit(0);
    }

    private void CleanupLeftoverStaging()
    {
        try
        {
            var appDir = _fileSystemService.GetAppDirectory();
            var stagingDir = Path.Combine(appDir, StagingDirName);
            if (Directory.Exists(stagingDir))
                Directory.Delete(stagingDir, true);
        }
        catch
        {
            // Best effort cleanup
        }
    }

    private static string NormalizeVersion(string version)
    {
        if (string.IsNullOrEmpty(version) || version == "dev")
            return version;

        // Format: v2026.02.05-7cd4d77 or v2026.02.05-7cd4d778a82e1e3cfcf2b3b3aed9dcb564e8a2c4
        // We need to normalize the SHA part to 7 characters
        var parts = version.Split('-');
        if (parts.Length != 2)
            return version;

        var datePart = parts[0]; // v2026.02.05
        var shaPart = parts[1];  // 7cd4d77 or full hash

        // If SHA is longer than 7 chars, truncate it
        if (shaPart.Length > 7)
            shaPart = shaPart[..7];

        return $"{datePart}-{shaPart}";
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = "";

        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }

        [JsonPropertyName("assets")]
        public List<GitHubAsset> Assets { get; set; } = [];
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = "";

        [JsonPropertyName("size")]
        public long Size { get; set; }
    }
}
