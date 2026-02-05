using System.Diagnostics;
using System.Text.Json;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Infrastructure.Services;

public class DockerContainerManager : IDockerContainerManager
{
    public async Task<IReadOnlyList<ContainerInfo>> GetContainersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await RunDockerCommandAsync("ps -a --format \"{{json .}}\"", cancellationToken);

            if (string.IsNullOrWhiteSpace(result))
                return [];

            var containers = new List<ContainerInfo>();
            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    var status = root.GetProperty("Status").GetString() ?? "";
                    containers.Add(new ContainerInfo
                    {
                        Id = root.GetProperty("ID").GetString() ?? "",
                        Name = root.GetProperty("Names").GetString() ?? "",
                        Image = root.GetProperty("Image").GetString() ?? "",
                        Status = status,
                        IsRunning = status.StartsWith("Up", StringComparison.OrdinalIgnoreCase)
                    });
                }
                catch (JsonException)
                {
                    // Skip malformed lines
                }
            }

            return containers;
        }
        catch
        {
            return [];
        }
    }

    public async Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await RunDockerCommandAsync($"start {containerId}", cancellationToken);
    }

    public async Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await RunDockerCommandAsync($"stop {containerId}", cancellationToken);
    }

    public async Task SwitchContainerAsync(string? currentContainerId, string newContainerId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(currentContainerId) && currentContainerId != newContainerId)
        {
            await StopContainerAsync(currentContainerId, cancellationToken);
        }

        await StartContainerAsync(newContainerId, cancellationToken);
    }

    private static async Task<string> RunDockerCommandAsync(string arguments, CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return output;
    }
}
