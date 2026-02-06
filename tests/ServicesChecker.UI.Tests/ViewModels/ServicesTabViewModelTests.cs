using System.Collections.ObjectModel;
using FluentAssertions;
using Moq;
using ServicesChecker.Application.Interfaces.Repositories;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Domain.Enums;
using ServicesChecker.UI.ViewModels;

namespace ServicesChecker.UI.Tests.ViewModels;

public class ServicesTabViewModelTests
{
    private readonly Mock<IServiceRepository> _mockServiceRepository;
    private readonly Mock<IWindowsServiceManager> _mockWindowsServiceManager;
    private readonly Mock<IRestEndpointChecker> _mockRestEndpointChecker;
    private readonly Mock<IDockerContainerManager> _mockContainerManager;
    private readonly Mock<ISettingsRepository> _mockSettingsRepository;
    private readonly Mock<IClipboardService> _mockClipboardService;

    public ServicesTabViewModelTests()
    {
        _mockServiceRepository = new Mock<IServiceRepository>();
        _mockWindowsServiceManager = new Mock<IWindowsServiceManager>();
        _mockRestEndpointChecker = new Mock<IRestEndpointChecker>();
        _mockContainerManager = new Mock<IDockerContainerManager>();
        _mockSettingsRepository = new Mock<ISettingsRepository>();
        _mockClipboardService = new Mock<IClipboardService>();

        // Setup default returns to prevent null reference exceptions
        _mockServiceRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceInfo>());
        _mockContainerManager.Setup(x => x.GetContainersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContainerInfo>());
        _mockSettingsRepository.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppSettings());
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Act
        var viewModel = CreateViewModel();
        Thread.Sleep(100); // Allow async initialization to start

        // Assert
        viewModel.Services.Should().NotBeNull();
        viewModel.Containers.Should().NotBeNull();
        viewModel.FilterText.Should().BeEmpty();
        viewModel.FilterConnectingToDb.Should().BeFalse();
    }

    [Fact]
    public async Task LoadServicesAsync_ShouldLoadServicesFromRepository()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new() { Name = "Service1", Status = ServiceStatus.Running, Type = ServiceType.WindowsService },
            new() { Name = "Service2", Status = ServiceStatus.Stopped, Type = ServiceType.RestEndpoint }
        };
        _mockServiceRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        // Assert
        _mockServiceRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public void FilterText_Changed_ShouldTriggerPropertyChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(ServicesTabViewModel.FilterText))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.FilterText = "test";

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void FilterConnectingToDb_Changed_ShouldTriggerPropertyChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(ServicesTabViewModel.FilterConnectingToDb))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.FilterConnectingToDb = true;

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void SelectedService_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var serviceItem = new ServiceItemViewModel { Name = "TestService" };

        // Act
        viewModel.SelectedService = serviceItem;

        // Assert
        viewModel.SelectedService.Should().Be(serviceItem);
    }

    [Fact]
    public void SelectedContainer_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var container = new ContainerInfo { Id = "123", Name = "TestContainer" };

        // Act
        viewModel.SelectedContainer = container;

        // Assert
        viewModel.SelectedContainer.Should().Be(container);
    }

    [Fact]
    public void NewServiceName_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.NewServiceName = "NewService";

        // Assert
        viewModel.NewServiceName.Should().Be("NewService");
    }

    [Fact]
    public void NewServiceIsRest_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.NewServiceIsRest = true;

        // Assert
        viewModel.NewServiceIsRest.Should().BeTrue();
    }

    [Fact]
    public void NewServiceConnectsToDb_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.NewServiceConnectsToDb = true;

        // Assert
        viewModel.NewServiceConnectsToDb.Should().BeTrue();
    }

    [Fact]
    public void Services_DefaultValue_ShouldBeEmptyCollection()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.Services.Should().NotBeNull();
        viewModel.Services.Should().BeOfType<ObservableCollection<ServiceItemViewModel>>();
    }

    [Fact]
    public void Containers_DefaultValue_ShouldBeEmptyCollection()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.Containers.Should().NotBeNull();
        viewModel.Containers.Should().BeOfType<ObservableCollection<ContainerInfo>>();
    }

    [Fact]
    public async Task LoadContainersAsync_ShouldCallContainerManager()
    {
        // Arrange
        var containers = new List<ContainerInfo>
        {
            new() { Id = "1", Name = "Container1" },
            new() { Id = "2", Name = "Container2" }
        };
        _mockContainerManager.Setup(x => x.GetContainersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(containers);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        // Assert
        _mockContainerManager.Verify(x => x.GetContainersAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public void IsBusy_ShouldInheritFromViewModelBase()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.IsBusy.Should().BeFalse(); // Default value from ViewModelBase
        viewModel.IsBusy = true;
        viewModel.IsBusy.Should().BeTrue();
    }

    [Fact]
    public void ErrorMessage_ShouldInheritFromViewModelBase()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.ErrorMessage.Should().BeNull(); // Default value from ViewModelBase
        viewModel.ErrorMessage = "Test error";
        viewModel.ErrorMessage.Should().Be("Test error");
    }

    [Fact]
    public async Task AddServiceAsync_WithValidWindowsService_ShouldAddServiceAndClearFilters()
    {
        // Arrange
        _mockWindowsServiceManager.Setup(x => x.ServiceExistsAsync("TestService", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockServiceRepository.Setup(x => x.AddAsync(It.IsAny<ServiceInfo>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        // Set filters before adding service
        viewModel.FilterText = "SomeFilter";
        viewModel.FilterConnectingToDb = true;
        viewModel.NewServiceName = "TestService";
        viewModel.NewServiceIsRest = false;
        viewModel.NewServiceConnectsToDb = false;

        // Act
        await viewModel.AddServiceCommand.ExecuteAsync(null);
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        viewModel.FilterText.Should().BeEmpty("filters should be cleared after adding a service");
        viewModel.FilterConnectingToDb.Should().BeFalse("filters should be cleared after adding a service");
        viewModel.NewServiceName.Should().BeEmpty("input fields should be cleared after adding a service");
        viewModel.NewServiceIsRest.Should().BeFalse();
        viewModel.NewServiceConnectsToDb.Should().BeFalse();
        _mockServiceRepository.Verify(x => x.AddAsync(It.Is<ServiceInfo>(s =>
            s.Name == "TestService" &&
            s.Type == ServiceType.WindowsService &&
            s.IsConnectingToDb == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddServiceAsync_WithRestEndpoint_ShouldAddServiceAndClearFilters()
    {
        // Arrange
        _mockServiceRepository.Setup(x => x.AddAsync(It.IsAny<ServiceInfo>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        // Set filters before adding service
        viewModel.FilterText = "SomeFilter";
        viewModel.FilterConnectingToDb = true;
        viewModel.NewServiceName = "http://example.com/api/health";
        viewModel.NewServiceIsRest = true;
        viewModel.NewServiceConnectsToDb = true;

        // Act
        await viewModel.AddServiceCommand.ExecuteAsync(null);
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        viewModel.FilterText.Should().BeEmpty("filters should be cleared after adding a service");
        viewModel.FilterConnectingToDb.Should().BeFalse("filters should be cleared after adding a service");
        viewModel.NewServiceName.Should().BeEmpty("input fields should be cleared after adding a service");
        viewModel.NewServiceIsRest.Should().BeFalse();
        viewModel.NewServiceConnectsToDb.Should().BeFalse();
        _mockServiceRepository.Verify(x => x.AddAsync(It.Is<ServiceInfo>(s =>
            s.Name == "http://example.com/api/health" &&
            s.Type == ServiceType.RestEndpoint &&
            s.IsConnectingToDb == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddServiceAsync_WithEmptyName_ShouldNotAddService()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        viewModel.NewServiceName = "   "; // Whitespace only

        // Act
        await viewModel.AddServiceCommand.ExecuteAsync(null);
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        _mockServiceRepository.Verify(x => x.AddAsync(It.IsAny<ServiceInfo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddServiceAsync_WithNonExistentWindowsService_ShouldShowError()
    {
        // Arrange
        _mockWindowsServiceManager.Setup(x => x.ServiceExistsAsync("NonExistentService", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        viewModel.NewServiceName = "NonExistentService";
        viewModel.NewServiceIsRest = false;

        // Act
        await viewModel.AddServiceCommand.ExecuteAsync(null);
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty("an error message should be displayed when service doesn't exist");
        viewModel.ErrorMessage.Should().Contain("NonExistentService");
        _mockServiceRepository.Verify(x => x.AddAsync(It.IsAny<ServiceInfo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CopyVersionAsync_WithValidVersion_ShouldCopyToClipboard()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        var serviceItem = new ServiceItemViewModel
        {
            Name = "TestService",
            Version = "1.2.3.4"
        };

        // Act
        await viewModel.CopyVersionCommand.ExecuteAsync(serviceItem);

        // Assert
        _mockClipboardService.Verify(x => x.SetTextAsync("1.2.3.4", It.IsAny<CancellationToken>()), Times.Once);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CopyVersionAsync_WithNullService_ShouldSetErrorMessage()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        // Act
        await viewModel.CopyVersionCommand.ExecuteAsync(null);

        // Assert
        _mockClipboardService.Verify(x => x.SetTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("No version available");
    }

    [Fact]
    public async Task CopyVersionAsync_WithNullVersion_ShouldSetErrorMessage()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        var serviceItem = new ServiceItemViewModel
        {
            Name = "TestService",
            Version = null
        };

        // Act
        await viewModel.CopyVersionCommand.ExecuteAsync(serviceItem);

        // Assert
        _mockClipboardService.Verify(x => x.SetTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("No version available");
    }

    [Fact]
    public async Task CopyVersionAsync_WithEmptyVersion_ShouldSetErrorMessage()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        var serviceItem = new ServiceItemViewModel
        {
            Name = "TestService",
            Version = "   " // Whitespace only
        };

        // Act
        await viewModel.CopyVersionCommand.ExecuteAsync(serviceItem);

        // Assert
        _mockClipboardService.Verify(x => x.SetTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("No version available");
    }

    [Fact]
    public async Task CopyVersionAsync_WhenClipboardServiceThrows_ShouldSetErrorMessage()
    {
        // Arrange
        _mockClipboardService.Setup(x => x.SetTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Clipboard not available"));

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        var serviceItem = new ServiceItemViewModel
        {
            Name = "TestService",
            Version = "1.2.3.4"
        };

        // Act
        await viewModel.CopyVersionCommand.ExecuteAsync(serviceItem);

        // Assert
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("Failed to copy version");
    }

    private ServicesTabViewModel CreateViewModel()
    {
        return new ServicesTabViewModel(
            _mockServiceRepository.Object,
            _mockWindowsServiceManager.Object,
            _mockRestEndpointChecker.Object,
            _mockContainerManager.Object,
            _mockSettingsRepository.Object,
            _mockClipboardService.Object);
    }
}
