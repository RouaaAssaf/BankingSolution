using Banking.Application.Abstractions;
using Banking.Messaging.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Customers.Api.Consumers
{
    public class TransactionCreatedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<TransactionCreatedConsumer> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;

        public TransactionCreatedConsumer(
            IServiceScopeFactory scopes,
            ILogger<TransactionCreatedConsumer> logger,
            IConfiguration cfg)
        {
            _scopes = scopes;
            _logger = logger;
            _exchange = cfg["RabbitMq:Exchange"] ?? "domain.events";
            var connStr = cfg["RabbitMq:ConnectionString"];
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

            // declare and bind queue
            _channel.QueueDeclare("customer-service.transaction-created", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("customer-service.transaction-created", _exchange, "transaction.created");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TransactionCreatedConsumer starting... Listening on queue: customer-service.transaction-created");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("Message received from RabbitMQ: {Message}", json);

                TransactionCreatedEvent? evt = null;
                try
                {
                    evt = JsonSerializer.Deserialize<TransactionCreatedEvent>(json);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize TransactionCreatedEvent. Raw message: {Message}", json);
                }

                if (evt != null)
                {
                    try
                    {
                        await HandleTransactionCreatedEventAsync(evt, stoppingToken);

                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling TransactionCreatedEvent.");
                        _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                    }
                }
                else
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume("customer-service.transaction-created", autoAck: false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleTransactionCreatedEventAsync(TransactionCreatedEvent evt, CancellationToken stoppingToken)
        {
            using var scope = _scopes.CreateScope();
            var customers = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();

            var customer = await customers.GetByIdAsync(evt.CustomerId, stoppingToken);
            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found. Cannot update balance.", evt.CustomerId);
                return;
            }

            // Update balance
            customer.Balance += evt.Amount; // Assuming positive = deposit, negative = withdrawal
            await customers.UpdateBalanceAsync(customer.Id, customer.Balance, stoppingToken);

            _logger.LogInformation("Updated balance for Customer {CustomerId} to {Balance}", customer.Id, customer.Balance);
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing TransactionCreatedConsumer resources.");
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
