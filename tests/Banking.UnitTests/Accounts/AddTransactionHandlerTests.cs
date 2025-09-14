using Moq;
using Xunit;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Banking.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class AddTransactionHandlerTests
{
    [Fact]
    public async Task Handle_Should_CreateTransaction_And_PublishEvents()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var account = new Account { Id = accountId, CustomerId = customerId, Balance = 1000m };

        var accountRepoMock = new Mock<IAccountRepository>();
        accountRepoMock.Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        accountRepoMock.Setup(x => x.AddTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction t, CancellationToken ct) => t.Amount + account.Balance);

        var publisherMock = new Mock<IEventPublisher>();
        publisherMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<AddTransactionHandler>>();

        var handler = new AddTransactionHandler(accountRepoMock.Object, publisherMock.Object, loggerMock.Object);

        var newAccountId = Guid.NewGuid();
        var command = new AddTransactionCommand(
            accountId,      // AccountId
            100m,           // Amount
            TransactionType.Credit,  // Type
            "Payment"       // Description
        );


        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        accountRepoMock.Verify(x => x.AddTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        publisherMock.Verify(x => x.PublishAsync("transaction.created", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        publisherMock.Verify(x => x.PublishAsync("account.balance.updated", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotEqual(Guid.Empty, result);
    }
}
