using System.Text;
using System.Text.Json;
using Transactions.Application.Interfaces;
using Banking.Messaging.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Accounts.Api.Consumers
{
    public class CustomerDeletedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CustomerDeletedConsumer> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;

        public CustomerDeletedConsumer(IServiceScopeFactory scopeFactory, ILogger<CustomerDeletedConsumer> logger, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            _exchange = config["RabbitMq:Exchange"] ?? "domain.events";
            var connStr = config["RabbitMq:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connStr))
                throw new InvalidOperationException("RabbitMq:ConnectionString is missing in configuration.");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connStr),
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true);

            _channel.QueueDeclare("account-service.customer-deleted", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("account-service.customer-deleted", _exchange, "customer.deleted");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CustomerDeletedConsumer started. Listening on 'customer-deleted' events...");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received message: {Message}", json);

                CustomerDeletedEvent? evt = null;
                try
                {
                    evt = JsonSerializer.Deserialize<CustomerDeletedEvent>(json);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize CustomerDeletedEvent.");
                }

                if (evt != null)
                {
                    try
                    {
                        await HandleCustomerDeletedAsync(evt, stoppingToken);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling CustomerDeletedEvent.");
                        _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                    }
                }
                else
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume("account-service.customer-deleted", false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleCustomerDeletedAsync(CustomerDeletedEvent evt, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var accountsRepo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var transactionsRepo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

            var accounts = await accountsRepo.GetAccountsByCustomerIdAsync(evt.CustomerId, ct);
            if (accounts == null || accounts.Count == 0)
            {
                _logger.LogInformation("No accounts found for deleted customer {CustomerId}.", evt.CustomerId);
                return;
            }

            foreach (var acc in accounts)
            {
                _logger.LogInformation("Deleting transactions for Account {AccountId}", acc.Id);
                await transactionsRepo.DeleteByAccountIdAsync(acc.Id, ct);

                _logger.LogInformation("Deleting Account {AccountId}", acc.Id);
                await accountsRepo.DeleteAsync(acc.Id, ct);
            }

            _logger.LogInformation("✅ Successfully deleted all accounts and transactions for Customer {CustomerId}", evt.CustomerId);
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
