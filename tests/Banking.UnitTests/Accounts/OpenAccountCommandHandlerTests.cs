using Banking.Application.Accounts.Commands;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Moq;
using Xunit;
using FluentAssertions;

namespace Banking.UnitTests.Accounts;

public class OpenAccountCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepo = new();
    private readonly Mock<IAccountRepository> _accountRepo = new();
    private readonly OpenAccountCommandHandler _sut;

    public OpenAccountCommandHandlerTests()
    {
        _sut = new OpenAccountCommandHandler(_customerRepo.Object, _accountRepo.Object);
    }

    [Fact]
    public async Task Handle_Should_CreateAccount_WhenCustomerExists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer { Id = customerId, FirstName = "Rouaa" };

        _customerRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _accountRepo.Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account acc, CancellationToken _) => acc);

        var command = new OpenAccountCommand(customerId, 0);

        // Act
        var accountId = await _sut.Handle(command, CancellationToken.None);

        // Assert
        accountId.Should().NotBeEmpty();
        _accountRepo.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCustomerDoesNotExist()
    {
        // Arrange
        var command = new OpenAccountCommand(Guid.NewGuid(), 0);

        _customerRepo.Setup(r => r.GetByIdAsync(command.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.Message.Contains("Customer not found"));
    }
}
