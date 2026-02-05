using FluentAssertions;
using Moq;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Domain.Enums;
using ServicesChecker.Infrastructure.Persistence;

namespace ServicesChecker.Infrastructure.Tests.Persistence;

public class JsonServiceRepositoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly Mock<IFileSystemService> _mockFileSystem;
    private readonly JsonServiceRepository _repository;

    public JsonServiceRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test_services_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        _mockFileSystem = new Mock<IFileSystemService>();
        _mockFileSystem.Setup(x => x.GetAppDirectory()).Returns(_testDirectory);

        _repository = new JsonServiceRepository(_mockFileSystem.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepository_ShouldReturnEmptyList()
    {
        // Act
        var services = await _repository.GetAllAsync();

        // Assert
        services.Should().NotBeNull();
        services.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAllAsync_WithServices_ShouldPersistToFile()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new() { Name = "Service1", Status = ServiceStatus.Running, Type = ServiceType.WindowsService },
            new() { Name = "Service2", Status = ServiceStatus.Stopped, Type = ServiceType.RestEndpoint }
        };

        // Act
        await _repository.SaveAllAsync(services);

        // Assert
        File.Exists(_testFilePath).Should().BeTrue();
        var savedServices = await _repository.GetAllAsync();
        savedServices.Should().HaveCount(2);
        savedServices[0].Name.Should().Be("Service1");
        savedServices[1].Name.Should().Be("Service2");
    }

    [Fact]
    public async Task GetAllAsync_AfterSave_ShouldReturnSavedServices()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new()
            {
                Name = "TestService",
                Status = ServiceStatus.Running,
                Type = ServiceType.WindowsService,
                Version = "1.0.0",
                IsConnectingToDb = true
            }
        };
        await _repository.SaveAllAsync(services);

        // Act
        var retrieved = await _repository.GetAllAsync();

        // Assert
        retrieved.Should().HaveCount(1);
        var service = retrieved[0];
        service.Name.Should().Be("TestService");
        service.Status.Should().Be(ServiceStatus.Running);
        service.Type.Should().Be(ServiceType.WindowsService);
        service.Version.Should().Be("1.0.0");
        service.IsConnectingToDb.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAllAsync_Overwrite_ShouldReplaceExistingData()
    {
        // Arrange
        var initialServices = new List<ServiceInfo>
        {
            new() { Name = "Service1", Status = ServiceStatus.Running }
        };
        await _repository.SaveAllAsync(initialServices);

        var newServices = new List<ServiceInfo>
        {
            new() { Name = "Service2", Status = ServiceStatus.Stopped },
            new() { Name = "Service3", Status = ServiceStatus.Paused }
        };

        // Act
        await _repository.SaveAllAsync(newServices);
        var retrieved = await _repository.GetAllAsync();

        // Assert
        retrieved.Should().HaveCount(2);
        retrieved.Should().NotContain(s => s.Name == "Service1");
        retrieved.Should().Contain(s => s.Name == "Service2");
        retrieved.Should().Contain(s => s.Name == "Service3");
    }

    [Fact]
    public async Task SaveAllAsync_EmptyList_ShouldClearRepository()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new() { Name = "Service1", Status = ServiceStatus.Running }
        };
        await _repository.SaveAllAsync(services);

        // Act
        await _repository.SaveAllAsync(new List<ServiceInfo>());
        var retrieved = await _repository.GetAllAsync();

        // Assert
        retrieved.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_FileNotExists_ShouldReturnEmptyList()
    {
        // Arrange
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }

        // Act
        var services = await _repository.GetAllAsync();

        // Assert
        services.Should().NotBeNull();
        services.Should().BeEmpty();
    }

    [Fact]
    public async Task ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        var service1 = new List<ServiceInfo>
        {
            new() { Name = "Service1", Status = ServiceStatus.Running }
        };
        var service2 = new List<ServiceInfo>
        {
            new() { Name = "Service2", Status = ServiceStatus.Stopped }
        };

        // Act - Concurrent writes
        var task1 = _repository.SaveAllAsync(service1);
        var task2 = _repository.SaveAllAsync(service2);
        await Task.WhenAll(task1, task2);

        // Assert - Should not throw and should have valid data
        var retrieved = await _repository.GetAllAsync();
        retrieved.Should().NotBeNull();
        retrieved.Should().HaveCountGreaterThan(0);
    }
}
