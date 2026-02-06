namespace ServicesChecker.Application.Interfaces.Services;

/// <summary>
/// Service for clipboard operations
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Copies text to the system clipboard
    /// </summary>
    /// <param name="text">Text to copy</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetTextAsync(string text, CancellationToken cancellationToken = default);
}
