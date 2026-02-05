using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Enums;

namespace ServicesChecker.Infrastructure.Services;

public class RestEndpointChecker : IRestEndpointChecker
{
    private readonly HttpClient _httpClient;

    public RestEndpointChecker(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<ServiceStatus> CheckHealthAsync(string url, CancellationToken cancellationToken = default)
    {
        var (status, _) = await CheckHealthWithResponseAsync(url, cancellationToken);
        return status;
    }

    public async Task<(ServiceStatus Status, string? ResponseBody)> CheckHealthWithResponseAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure URL has a protocol
            var normalizedUrl = NormalizeUrl(url);

            using var response = await _httpClient.GetAsync(normalizedUrl, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return (ServiceStatus.Available, body);
            }

            return (ServiceStatus.Unavailable, body);
        }
        catch (HttpRequestException)
        {
            return (ServiceStatus.Unavailable, null);
        }
        catch (TaskCanceledException)
        {
            return (ServiceStatus.Unavailable, null);
        }
        catch (Exception)
        {
            return (ServiceStatus.Error, null);
        }
    }

    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url;

        // If URL doesn't start with http:// or https://, add https://
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return $"https://{url}";
        }

        return url;
    }
}
