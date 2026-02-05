using FluentAssertions;
using Moq;
using Moq.Protected;
using ServicesChecker.Domain.Enums;
using ServicesChecker.Infrastructure.Services;
using System.Net;

namespace ServicesChecker.Infrastructure.Tests.Services;

public class RestEndpointCheckerTests
{
    private Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, string? content = null)
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
                Content = content != null ? new StringContent(content) : new StringContent(string.Empty)
            });
        return mockHandler;
    }

    [Fact]
    public async Task CheckHealthAsync_SuccessfulResponse_ShouldReturnAvailable()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, "OK");
        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        var result = await checker.CheckHealthAsync("https://api.example.com/health");

        // Assert
        result.Should().Be(ServiceStatus.Available);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    public async Task CheckHealthAsync_SuccessStatusCodes_ShouldReturnAvailable(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(statusCode);
        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        var result = await checker.CheckHealthAsync("https://api.example.com");

        // Assert
        result.Should().Be(ServiceStatus.Available);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task CheckHealthAsync_ErrorStatusCodes_ShouldReturnUnavailable(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(statusCode);
        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        var result = await checker.CheckHealthAsync("https://api.example.com");

        // Assert
        result.Should().Be(ServiceStatus.Unavailable);
    }

    [Fact]
    public async Task CheckHealthAsync_HttpRequestException_ShouldReturnUnavailable()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        var result = await checker.CheckHealthAsync("https://api.example.com");

        // Assert
        result.Should().Be(ServiceStatus.Unavailable);
    }

    [Fact]
    public async Task CheckHealthAsync_TaskCanceledException_ShouldReturnUnavailable()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        var result = await checker.CheckHealthAsync("https://api.example.com");

        // Assert
        result.Should().Be(ServiceStatus.Unavailable);
    }

    [Fact]
    public async Task CheckHealthAsync_GenericException_ShouldReturnError()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        var result = await checker.CheckHealthAsync("https://api.example.com");

        // Assert
        result.Should().Be(ServiceStatus.Error);
    }

    [Fact]
    public async Task CheckHealthAsync_UrlWithoutProtocol_ShouldAddHttps()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        var result = await checker.CheckHealthAsync("api.example.com/health");

        // Assert
        result.Should().Be(ServiceStatus.Available);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri != null &&
                req.RequestUri.ToString().StartsWith("https://api.example.com")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task CheckHealthAsync_WithCancellationToken_ShouldPassTokenToHttpClient()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);
        var cts = new CancellationTokenSource();

        // Act
        await checker.CheckHealthAsync("https://api.example.com", cts.Token);

        // Assert
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.Is<CancellationToken>(ct => ct == cts.Token));
    }

    [Fact]
    public async Task CheckHealthWithResponseAsync_SuccessfulResponse_ShouldReturnAvailableWithBody()
    {
        // Arrange
        var expectedBody = "{\"status\":\"healthy\"}";
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, expectedBody);
        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        var (status, body) = await checker.CheckHealthWithResponseAsync("https://api.example.com");

        // Assert
        status.Should().Be(ServiceStatus.Available);
        body.Should().Be(expectedBody);
    }

    [Fact]
    public async Task CheckHealthWithResponseAsync_ErrorResponse_ShouldReturnUnavailableWithBody()
    {
        // Arrange
        var expectedBody = "{\"error\":\"Service unavailable\"}";
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.ServiceUnavailable, expectedBody);
        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        var (status, body) = await checker.CheckHealthWithResponseAsync("https://api.example.com");

        // Assert
        status.Should().Be(ServiceStatus.Unavailable);
        body.Should().Be(expectedBody);
    }

    [Fact]
    public async Task CheckHealthWithResponseAsync_HttpRequestException_ShouldReturnUnavailableWithNullBody()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        var (status, body) = await checker.CheckHealthWithResponseAsync("https://api.example.com");

        // Assert
        status.Should().Be(ServiceStatus.Unavailable);
        body.Should().BeNull();
    }

    [Fact]
    public async Task CheckHealthWithResponseAsync_GenericException_ShouldReturnErrorWithNullBody()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        var (status, body) = await checker.CheckHealthWithResponseAsync("https://api.example.com");

        // Assert
        status.Should().Be(ServiceStatus.Error);
        body.Should().BeNull();
    }

    [Theory]
    [InlineData("https://example.com", "https://example.com")]
    [InlineData("http://example.com", "http://example.com")]
    [InlineData("example.com", "https://example.com")]
    [InlineData("api.service.local", "https://api.service.local")]
    public async Task CheckHealthAsync_UrlNormalization_ShouldHandleVariousFormats(string inputUrl, string expectedUrl)
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new RestEndpointChecker(httpClient);

        // Act
        await checker.CheckHealthAsync(inputUrl);

        // Assert
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri != null &&
                req.RequestUri.ToString().StartsWith(expectedUrl)),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public void Constructor_ShouldSetTimeoutTo10Seconds()
    {
        // Arrange & Act
        var httpClient = new HttpClient();
        var checker = new RestEndpointChecker(httpClient);

        // Assert
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(10));
    }
}
