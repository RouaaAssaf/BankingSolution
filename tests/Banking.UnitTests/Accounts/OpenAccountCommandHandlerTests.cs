using Banking.Application.Accounts.Commands;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Banking.UnitTests.Accounts;

public class OpenAccountCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepo = new();
    private readonly Mock<IAccountRepository> _accountRepo = new();
    private readonly Mock<ITransactionRepository> _transactionRepo = new();
    private readonly OpenAccountCommandHandler _sut;

    public OpenAccountCommandHandlerTests()
    {
        _sut = new OpenAccountCommandHandler(
            _customerRepo.Object,
            _accountRepo.Object,
            _transactionRepo.Object);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Customer_Not_Found()
    {
        // Arrange
        var command = new OpenAccountCommand(Guid.NewGuid(), 0);
        _customerRepo.Setup(r => r.GetByIdAsync(command.CustomerId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Customer not found");
    }

    [Fact]
    public async Task Handle_Should_Create_Account_When_Customer_Exists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new OpenAccountCommand(customerId, 0);

        _customerRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new Customer { Id = customerId, FirstName = "John", LastName = "Doe" });

        // Act
        var accountId = await _sut.Handle(command, CancellationToken.None);

        // Assert
        _accountRepo.Verify(r => r.AddAsync(It.Is<Account>(a =>
            a.Id == accountId &&
            a.CustomerId == customerId
        ), It.IsAny<CancellationToken>()), Times.Once);

        _transactionRepo.Verify(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Create_Transaction_When_InitialDeposit_GreaterThanZero()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new OpenAccountCommand(customerId, 100);

        _customerRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new Customer { Id = customerId, FirstName = "John", LastName = "Doe" });

        // Act
        var accountId = await _sut.Handle(command, CancellationToken.None);

        // Assert
        _transactionRepo.Verify(r => r.AddAsync(It.Is<Transaction>(t =>
            t.AccountId == accountId &&
            t.Amount == 100 &&
            t.Type == TransactionType.Credit &&
            t.Description == "Initial Credit"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
