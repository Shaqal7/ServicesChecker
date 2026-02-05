using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Infrastructure.Services;

namespace ServicesChecker.Infrastructure.Tests.Services;

public class GitHubUpdateServiceTests
{
    private readonly Mock<IFileSystemService> _mockFileSystemService;

    private const string SampleReleaseJson = """
        {
            "tag_name": "v2026.02.06-abc1234",
            "name": "ServicesChecker v2026.02.06-abc1234",
            "html_url": "https://github.com/Shaqal7/ServicesChecker/releases/tag/v2026.02.06-abc1234",
            "published_at": "2026-02-06T12:00:00Z",
            "assets": [
                {
                    "name": "ServicesChecker-win-x64.zip",
                    "browser_download_url": "https://github.com/Shaqal7/ServicesChecker/releases/download/v2026.02.06-abc1234/ServicesChecker-win-x64.zip",
                    "size": 52428800
                }
            ]
        }
        """;

    public GitHubUpdateServiceTests()
    {
        _mockFileSystemService = new Mock<IFileSystemService>();
        _mockFileSystemService.Setup(x => x.GetAppDirectory()).Returns("C:\\TestApp\\");
    }

    private static Mock<HttpMessageHandler> CreateMockHandler(HttpStatusCode statusCode, string content)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
        return mockHandler;
    }

    private static Mock<HttpMessageHandler> CreateThrowingHandler(Exception exception)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);
        return mockHandler;
    }

    private GitHubUpdateService CreateService(HttpClient httpClient)
    {
        return new GitHubUpdateService(httpClient, _mockFileSystemService.Object);
    }

    [Fact]
    public void GetCurrentVersion_ShouldReturnNonNullString()
    {
        // Arrange
        var mockHandler = CreateMockHandler(HttpStatusCode.OK, "{}");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = CreateService(httpClient);

        // Act
        var version = service.GetCurrentVersion();

        // Assert
        version.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenApiReturnsNewerVersion_ReturnsUpdateInfo()
    {
        // Arrange
        var mockHandler = CreateMockHandler(HttpStatusCode.OK, SampleReleaseJson);
        var httpClient = new HttpClient(mockHandler.Object);
        var service = CreateService(httpClient);

        // Act
        var result = await service.CheckForUpdateAsync();

        // In test context, GetCurrentVersion() returns the test runner version (not "dev"),
        // which differs from the mock release tag, so an update is detected.
        if (service.GetCurrentVersion() == "dev")
        {
            result.Should().BeNull();
        }
        else
        {
            result.Should().NotBeNull();
            result!.TagName.Should().Be("v2026.02.06-abc1234");
            result.ReleaseName.Should().Be("ServicesChecker v2026.02.06-abc1234");
            result.DownloadUrl.Should().Contain("ServicesChecker-win-x64.zip");
            result.AssetSizeBytes.Should().Be(52428800);
            result.IsNewerThanCurrent.Should().BeTrue();
        }
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenNetworkFails_ReturnsNull()
    {
        // Arrange
        var mockHandler = CreateThrowingHandler(new HttpRequestException("Network error"));
        var httpClient = new HttpClient(mockHandler.Object);
        var service = CreateService(httpClient);

        // Act
        var result = await service.CheckForUpdateAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenApiReturnsInvalidJson_ReturnsNull()
    {
        // Arrange
        var mockHandler = CreateMockHandler(HttpStatusCode.OK, "not valid json!!!");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = CreateService(httpClient);

        // Act
        var result = await service.CheckForUpdateAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenApiReturns404_ReturnsNull()
    {
        // Arrange
        var mockHandler = CreateMockHandler(HttpStatusCode.NotFound, "");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = CreateService(httpClient);

        // Act
        var result = await service.CheckForUpdateAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenNoMatchingAsset_ReturnsNull()
    {
        // Arrange
        var jsonNoAsset = """
            {
                "tag_name": "v2026.02.06-abc1234",
                "name": "Test Release",
                "html_url": "https://github.com/test",
                "published_at": "2026-02-06T12:00:00Z",
                "assets": [
                    {
                        "name": "some-other-file.tar.gz",
                        "browser_download_url": "https://example.com/other.tar.gz",
                        "size": 1000
                    }
                ]
            }
            """;
        var mockHandler = CreateMockHandler(HttpStatusCode.OK, jsonNoAsset);
        var httpClient = new HttpClient(mockHandler.Object);
        var service = CreateService(httpClient);

        // Act
        var result = await service.CheckForUpdateAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenEmptyTagName_ReturnsNull()
    {
        // Arrange
        var jsonEmptyTag = """
            {
                "tag_name": "",
                "name": "Test",
                "html_url": "https://github.com/test",
                "published_at": "2026-02-06T12:00:00Z",
                "assets": []
            }
            """;
        var mockHandler = CreateMockHandler(HttpStatusCode.OK, jsonEmptyTag);
        var httpClient = new HttpClient(mockHandler.Object);
        var service = CreateService(httpClient);

        // Act
        var result = await service.CheckForUpdateAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenTimeout_ReturnsNull()
    {
        // Arrange
        var mockHandler = CreateThrowingHandler(new TaskCanceledException("Request timed out"));
        var httpClient = new HttpClient(mockHandler.Object);
        var service = CreateService(httpClient);

        // Act
        var result = await service.CheckForUpdateAsync();

        // Assert
        result.Should().BeNull();
    }
}
