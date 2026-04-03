using FluentAssertions;
using Moq;
using TaskBoard.Api.Logic.Requests.Auth;
using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure.Auth;
using TaskBoard.Infrastructure.Cache;
using TaskBoard.Infrastructure.Repositories;
using Xunit;

namespace TaskBoard.Tests.Handlers;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IInMemoryRedis> _redisMock = new();

    private LoginCommandHandler CreateHandler() =>
        new(_userRepositoryMock.Object, _passwordHasherMock.Object, _redisMock.Object);

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsOneTimeCode()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@taskboard.local",
            PasswordHash = "$2a$11$coXE/FGO9I0Rm4u.yQt/KuxHyS8dVrggKCdGk0i03EeuFLWv4Z2X6",
            IsActive = true,
            Roles = new List<Role> { new() { Id = Guid.NewGuid(), Name = "Admin" } }
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("admin@taskboard.local"))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.VerifyPassword("Password123!", user.PasswordHash))
            .Returns(true);

        _redisMock
            .Setup(r => r.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();
        var command = new LoginCommand { Email = "admin@taskboard.local", Password = "Password123!" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.OneTimeCode.Should().NotBeNullOrEmpty();
        _redisMock.Verify(r => r.SetAsync(
            It.Is<string>(k => k.StartsWith("otc:")),
            user.Id.ToString(),
            It.Is<TimeSpan?>(t => t.HasValue && t.Value == TimeSpan.FromSeconds(60))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNull()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var command = new LoginCommand { Email = "nonexistent@example.com", Password = "SomePassword" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _redisMock.Verify(r => r.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsNull()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@taskboard.local",
            PasswordHash = "$2a$11$coXE/FGO9I0Rm4u.yQt/KuxHyS8dVrggKCdGk0i03EeuFLWv4Z2X6",
            IsActive = true,
            Roles = new List<Role>()
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("admin@taskboard.local"))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.VerifyPassword("WrongPassword", user.PasswordHash))
            .Returns(false);

        var handler = CreateHandler();
        var command = new LoginCommand { Email = "admin@taskboard.local", Password = "WrongPassword" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _redisMock.Verify(r => r.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Never);
    }
}
