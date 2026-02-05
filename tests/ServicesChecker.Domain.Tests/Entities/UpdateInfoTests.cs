using FluentAssertions;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Domain.Tests.Entities;

public class UpdateInfoTests
{
    [Fact]
    public void UpdateInfo_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var updateInfo = new UpdateInfo();

        // Assert
        updateInfo.TagName.Should().BeEmpty();
        updateInfo.ReleaseName.Should().BeEmpty();
        updateInfo.HtmlUrl.Should().BeEmpty();
        updateInfo.DownloadUrl.Should().BeEmpty();
        updateInfo.AssetSizeBytes.Should().Be(0);
        updateInfo.PublishedAt.Should().Be(default);
        updateInfo.IsNewerThanCurrent.Should().BeFalse();
    }

    [Fact]
    public void UpdateInfo_SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var updateInfo = new UpdateInfo();
        var publishedAt = new DateTime(2026, 2, 6, 12, 0, 0);

        // Act
        updateInfo.TagName = "v2026.02.06-abc1234";
        updateInfo.ReleaseName = "ServicesChecker v2026.02.06-abc1234";
        updateInfo.HtmlUrl = "https://github.com/Shaqal7/ServicesChecker/releases/tag/v2026.02.06-abc1234";
        updateInfo.DownloadUrl = "https://github.com/Shaqal7/ServicesChecker/releases/download/v2026.02.06-abc1234/ServicesChecker-win-x64.zip";
        updateInfo.AssetSizeBytes = 52428800;
        updateInfo.PublishedAt = publishedAt;
        updateInfo.IsNewerThanCurrent = true;

        // Assert
        updateInfo.TagName.Should().Be("v2026.02.06-abc1234");
        updateInfo.ReleaseName.Should().Be("ServicesChecker v2026.02.06-abc1234");
        updateInfo.HtmlUrl.Should().Contain("github.com");
        updateInfo.DownloadUrl.Should().Contain("ServicesChecker-win-x64.zip");
        updateInfo.AssetSizeBytes.Should().Be(52428800);
        updateInfo.PublishedAt.Should().Be(publishedAt);
        updateInfo.IsNewerThanCurrent.Should().BeTrue();
    }

    [Fact]
    public void UpdateInfo_WithFullDetails_ShouldStoreAllProperties()
    {
        // Arrange & Act
        var updateInfo = new UpdateInfo
        {
            TagName = "v2026.01.15-def5678",
            ReleaseName = "January Release",
            HtmlUrl = "https://github.com/Shaqal7/ServicesChecker/releases/tag/v2026.01.15-def5678",
            DownloadUrl = "https://github.com/Shaqal7/ServicesChecker/releases/download/v2026.01.15-def5678/ServicesChecker-win-x64.zip",
            AssetSizeBytes = 100_000_000,
            PublishedAt = new DateTime(2026, 1, 15),
            IsNewerThanCurrent = true
        };

        // Assert
        updateInfo.TagName.Should().Be("v2026.01.15-def5678");
        updateInfo.ReleaseName.Should().Be("January Release");
        updateInfo.AssetSizeBytes.Should().Be(100_000_000);
        updateInfo.IsNewerThanCurrent.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateInfo_IsNewerThanCurrent_ShouldStoreCorrectly(bool isNewer)
    {
        // Arrange & Act
        var updateInfo = new UpdateInfo { IsNewerThanCurrent = isNewer };

        // Assert
        updateInfo.IsNewerThanCurrent.Should().Be(isNewer);
    }
}
