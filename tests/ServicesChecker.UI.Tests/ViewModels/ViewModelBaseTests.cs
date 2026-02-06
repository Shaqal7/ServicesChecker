using FluentAssertions;
using ServicesChecker.UI.ViewModels;

namespace ServicesChecker.UI.Tests.ViewModels;

/// <summary>
/// Test ViewModel that inherits from ViewModelBase for testing purposes
/// </summary>
public partial class TestViewModel : ViewModelBase
{
    public void PublicClearError() => ClearError();
    public void PublicSetError(string message) => SetError(message);
}

public class ViewModelBaseTests
{
    [Fact]
    public void IsBusy_DefaultValue_ShouldBeFalse()
    {
        // Arrange & Act
        var viewModel = new TestViewModel();

        // Assert
        viewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public void IsBusy_SetToTrue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new TestViewModel();

        // Act
        viewModel.IsBusy = true;

        // Assert
        viewModel.IsBusy.Should().BeTrue();
    }

    [Fact]
    public void IsBusy_SetToFalse_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new TestViewModel { IsBusy = true };

        // Act
        viewModel.IsBusy = false;

        // Assert
        viewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public void IsBusy_PropertyChanged_ShouldRaiseNotification()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(TestViewModel.IsBusy))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.IsBusy = true;

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void ErrorMessage_DefaultValue_ShouldBeNull()
    {
        // Arrange & Act
        var viewModel = new TestViewModel();

        // Assert
        viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ErrorMessage_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new TestViewModel();
        const string errorMessage = "An error occurred";

        // Act
        viewModel.ErrorMessage = errorMessage;

        // Assert
        viewModel.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void ErrorMessage_SetToNull_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new TestViewModel { ErrorMessage = "Initial error" };

        // Act
        viewModel.ErrorMessage = null;

        // Assert
        viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ErrorMessage_PropertyChanged_ShouldRaiseNotification()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(TestViewModel.ErrorMessage))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.ErrorMessage = "Error";

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void SetError_WithMessage_ShouldSetErrorMessage()
    {
        // Arrange
        var viewModel = new TestViewModel();
        const string errorMessage = "Operation failed";

        // Act
        viewModel.PublicSetError(errorMessage);

        // Assert
        viewModel.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void SetError_MultipleTimes_ShouldUpdateErrorMessage()
    {
        // Arrange
        var viewModel = new TestViewModel();
        const string firstError = "First error";
        const string secondError = "Second error";

        // Act
        viewModel.PublicSetError(firstError);
        viewModel.PublicSetError(secondError);

        // Assert
        viewModel.ErrorMessage.Should().Be(secondError);
    }

    [Fact]
    public void ClearError_WhenErrorMessageSet_ShouldSetToNull()
    {
        // Arrange
        var viewModel = new TestViewModel { ErrorMessage = "Some error" };

        // Act
        viewModel.PublicClearError();

        // Assert
        viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ClearError_WhenErrorMessageNull_ShouldRemainNull()
    {
        // Arrange
        var viewModel = new TestViewModel();

        // Act
        viewModel.PublicClearError();

        // Assert
        viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ClearError_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel { ErrorMessage = "Error" };
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(TestViewModel.ErrorMessage))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.PublicClearError();

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void SetError_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(TestViewModel.ErrorMessage))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.PublicSetError("New error");

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Error message")]
    [InlineData("A very long error message with lots of details about what went wrong")]
    public void SetError_WithVariousMessages_ShouldSetCorrectly(string errorMessage)
    {
        // Arrange
        var viewModel = new TestViewModel();

        // Act
        viewModel.PublicSetError(errorMessage);

        // Assert
        viewModel.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void DismissErrorCommand_ShouldExist()
    {
        // Arrange
        var viewModel = new TestViewModel();

        // Act & Assert
        viewModel.DismissErrorCommand.Should().NotBeNull();
    }

    [Fact]
    public void DismissErrorCommand_WhenErrorMessageSet_ShouldClearError()
    {
        // Arrange
        var viewModel = new TestViewModel { ErrorMessage = "Test error message" };

        // Act
        viewModel.DismissErrorCommand.Execute(null);

        // Assert
        viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void DismissErrorCommand_WhenErrorMessageNull_ShouldRemainNull()
    {
        // Arrange
        var viewModel = new TestViewModel();

        // Act
        viewModel.DismissErrorCommand.Execute(null);

        // Assert
        viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void DismissErrorCommand_MultipleTimes_ShouldWork()
    {
        // Arrange
        var viewModel = new TestViewModel { ErrorMessage = "Error 1" };

        // Act
        viewModel.DismissErrorCommand.Execute(null);
        viewModel.ErrorMessage = "Error 2";
        viewModel.DismissErrorCommand.Execute(null);

        // Assert
        viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void DismissErrorCommand_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel { ErrorMessage = "Error message" };
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(TestViewModel.ErrorMessage))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.DismissErrorCommand.Execute(null);

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void DismissErrorCommand_CanExecute_ShouldAlwaysBeTrue()
    {
        // Arrange
        var viewModel = new TestViewModel();

        // Act & Assert
        viewModel.DismissErrorCommand.CanExecute(null).Should().BeTrue();

        // Set error and test again
        viewModel.ErrorMessage = "Error";
        viewModel.DismissErrorCommand.CanExecute(null).Should().BeTrue();
    }
}
