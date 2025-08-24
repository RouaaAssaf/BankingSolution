using Banking.Application.Accounts;
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Moq;
using Xunit;
using FluentAssertions;

namespace Banking.UnitTests.Accounts;

public class OpenAccountServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepo = new();
    private readonly Mock<IAccountRepository> _accountRepo = new();
    private readonly OpenAccountService _sut; // System Under Test

    public OpenAccountServiceTests()
    {
        _sut = new OpenAccountService(_customerRepo.Object, _accountRepo.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_CreateAccount_WhenCustomerExists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer { Id = customerId, FirstName = "Rouaa" };

        _customerRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _accountRepo.Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account acc, CancellationToken _) => acc);

        var request = new OpenAccountRequest()
        {
            CustomerId = customerId,
            InitialCredit = 0
        };

        // Act
        var accountId = await _sut.HandleAsync(request, CancellationToken.None);

        // Assert
        accountId.Should().NotBeEmpty();
        _accountRepo.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCustomerDoesNotExist()
    {
        // Arrange
        var request = new OpenAccountRequest
        {
            CustomerId = Guid.NewGuid(),
            InitialCredit = 0
        };

        _customerRepo.Setup(r => r.GetByIdAsync(request.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = () => _sut.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
    .Where(ex => ex.Message.Contains("Customer not found"));
    }
}
