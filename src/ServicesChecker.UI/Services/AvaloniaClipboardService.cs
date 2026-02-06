using ServicesChecker.Application.Interfaces.Services;
using TextCopy;

namespace ServicesChecker.UI.Services;

/// <summary>
/// Cross-platform clipboard service implementation using TextCopy
/// </summary>
public class AvaloniaClipboardService : IClipboardService
{
    /// <inheritdoc/>
    public async Task SetTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentException("Text cannot be null or empty", nameof(text));
        }

        await ClipboardService.SetTextAsync(text, cancellationToken);
    }
}
