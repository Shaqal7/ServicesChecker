namespace ServicesChecker.Domain.Entities;

public class UpdateInfo
{
    public string TagName { get; set; } = string.Empty;

    public string ReleaseName { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public string DownloadUrl { get; set; } = string.Empty;

    public long AssetSizeBytes { get; set; }

    public DateTime PublishedAt { get; set; }

    public bool IsNewerThanCurrent { get; set; }
}
