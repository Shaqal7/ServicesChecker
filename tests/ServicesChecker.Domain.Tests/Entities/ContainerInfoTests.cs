using FluentAssertions;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Domain.Tests.Entities;

public class ContainerInfoTests
{
    [Fact]
    public void ContainerInfo_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var containerInfo = new ContainerInfo();

        // Assert
        containerInfo.Id.Should().BeEmpty();
        containerInfo.Name.Should().BeEmpty();
        containerInfo.Image.Should().BeEmpty();
        containerInfo.Status.Should().BeEmpty();
        containerInfo.IsRunning.Should().BeFalse();
        containerInfo.CreatedAt.Should().BeNull();
    }

    [Fact]
    public void ContainerInfo_SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var containerInfo = new ContainerInfo();
        var createdAt = DateTime.Now.AddDays(-7);

        // Act
        containerInfo.Id = "abc123";
        containerInfo.Name = "my-container";
        containerInfo.Image = "nginx:latest";
        containerInfo.Status = "running";
        containerInfo.IsRunning = true;
        containerInfo.CreatedAt = createdAt;

        // Assert
        containerInfo.Id.Should().Be("abc123");
        containerInfo.Name.Should().Be("my-container");
        containerInfo.Image.Should().Be("nginx:latest");
        containerInfo.Status.Should().Be("running");
        containerInfo.IsRunning.Should().BeTrue();
        containerInfo.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ContainerInfo_RunningContainer_ShouldHaveIsRunningTrue()
    {
        // Arrange & Act
        var containerInfo = new ContainerInfo
        {
            Id = "container1",
            Name = "web-server",
            Status = "running",
            IsRunning = true
        };

        // Assert
        containerInfo.IsRunning.Should().BeTrue();
        containerInfo.Status.Should().Be("running");
    }

    [Fact]
    public void ContainerInfo_StoppedContainer_ShouldHaveIsRunningFalse()
    {
        // Arrange & Act
        var containerInfo = new ContainerInfo
        {
            Id = "container2",
            Name = "database",
            Status = "exited",
            IsRunning = false
        };

        // Assert
        containerInfo.IsRunning.Should().BeFalse();
        containerInfo.Status.Should().Be("exited");
    }

    [Theory]
    [InlineData("running", true)]
    [InlineData("exited", false)]
    [InlineData("paused", false)]
    [InlineData("restarting", false)]
    public void ContainerInfo_DifferentStatuses_ShouldStoreCorrectly(string status, bool expectedIsRunning)
    {
        // Arrange & Act
        var containerInfo = new ContainerInfo
        {
            Status = status,
            IsRunning = expectedIsRunning
        };

        // Assert
        containerInfo.Status.Should().Be(status);
        containerInfo.IsRunning.Should().Be(expectedIsRunning);
    }

    [Fact]
    public void ContainerInfo_WithFullDetails_ShouldStoreAllProperties()
    {
        // Arrange
        var createdAt = new DateTime(2024, 1, 1, 12, 0, 0);

        // Act
        var containerInfo = new ContainerInfo
        {
            Id = "1234567890abcdef",
            Name = "my-app-container",
            Image = "myapp:v1.2.3",
            Status = "running",
            IsRunning = true,
            CreatedAt = createdAt
        };

        // Assert
        containerInfo.Id.Should().Be("1234567890abcdef");
        containerInfo.Name.Should().Be("my-app-container");
        containerInfo.Image.Should().Be("myapp:v1.2.3");
        containerInfo.Status.Should().Be("running");
        containerInfo.IsRunning.Should().BeTrue();
        containerInfo.CreatedAt.Should().Be(createdAt);
    }
}
