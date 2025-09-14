using Banking.Domain.Entities;
using Banking.Messaging;
using Banking.Messaging.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Transactions.Api.Consumers;
public class CustomerCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IConfiguration _cfg;
    private readonly IConnection _conn;
    private readonly IModel _chan;
    private readonly string _exchange;
    private readonly ILogger<CustomerCreatedConsumer> _logger;

    public CustomerCreatedConsumer(IServiceScopeFactory scopes, IConfiguration cfg, ILogger<CustomerCreatedConsumer> logger)
    {
        _scopes = scopes;
        _cfg = cfg;
        _exchange = cfg["RabbitMq:Exchange"] ?? "domain.events";

        var connStr = cfg["RabbitMq:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connStr))
        {
            throw new InvalidOperationException("RabbitMq:ConnectionString is missing in configuration.");
        }

        var factory = new ConnectionFactory
        {
            Uri = new Uri(connStr),
            DispatchConsumersAsync = true
        };

        _conn = factory.CreateConnection();
        _chan = _conn.CreateModel();
        _chan.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true);
        _chan.QueueDeclare("account-service.customer-created", durable: true, exclusive: false, autoDelete: false);
        _chan.QueueBind("account-service.customer-created", _exchange, "customer.created");
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CustomerCreatedConsumer starting... Listening on queue: {Queue}", "account-service.customer-created");

        var consumer = new AsyncEventingBasicConsumer(_chan);
        consumer.Received += async (s, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation("Message received from RabbitMQ: {Message}", json);

            CustomerCreatedEvent? evt = null;
            try
            {
                evt = JsonSerializer.Deserialize<CustomerCreatedEvent>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize CustomerCreatedEvent. Raw message: {Message}", json);
            }

            if (evt == null)
            {
                _logger.LogWarning("Skipping message — could not deserialize into CustomerCreatedEvent.");
                _chan!.BasicAck(ea.DeliveryTag, false);
                return;
            }

            using var scope = _scopes.CreateScope();
            var accounts = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            // Check for existing account
            var existing = await accounts.GetByCustomerIdAsync(evt.CustomerId, stoppingToken);
            if (existing != null)
            {
                _logger.LogInformation("Account already exists for CustomerId {CustomerId}. Skipping creation.", evt.CustomerId);
                _chan.BasicAck(ea.DeliveryTag, false);
                return;
            }

            // Create new account
            var account = new Account
            {
                Id = Guid.NewGuid(),
                CustomerId = evt.CustomerId,
                OpenedAt = DateTime.UtcNow,
                Balance = 0m
            };

            _logger.LogInformation("Creating new account {AccountId} for Customer {CustomerId}", account.Id, account.CustomerId);

            await accounts.AddAsync(account, stoppingToken);
            await accounts.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Account {AccountId} saved to repository.", account.Id);

            // Publish AccountCreated event
            var accountCreated = new AccountCreatedEvent(account.Id, account.CustomerId, account.Balance, DateTime.UtcNow);
            await publisher.PublishAsync("account.created", accountCreated, stoppingToken);
            _logger.LogInformation("Published AccountCreatedEvent for Account {AccountId}, Customer {CustomerId}", account.Id, account.CustomerId);

            _chan.BasicAck(ea.DeliveryTag, false);
            _logger.LogInformation("Message acknowledged.");
        };

        _chan.BasicConsume("account-service.customer-created", autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _logger.LogInformation("Disposing CustomerCreatedConsumer resources.");
        _chan?.Close();
        _conn?.Close();
        base.Dispose();
    }
}
