using Transactions.Domain.Entities;
using Banking.Messaging;
using Banking.Messaging.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;


namespace Transactions.Api.Consumers
{
    public class CustomerCreatedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<CustomerCreatedConsumer> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;

        public CustomerCreatedConsumer(IServiceScopeFactory scopes, ILogger<CustomerCreatedConsumer> logger, IConfiguration cfg)
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
            _channel.QueueDeclare("account-service.customer-created", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("account-service.customer-created", _exchange, "customer.created");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CustomerCreatedConsumer starting... Listening on queue: account-service.customer-created");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
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

                if (evt != null)
                {
                    try
                    {
                        // safe async processing in a separate method
                        await HandleCustomerCreatedEventAsync(evt, stoppingToken);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling CustomerCreatedEvent.");
                        _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                    }
                }
                else
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume("account-service.customer-created", autoAck: false, consumer);
            return Task.CompletedTask;
        }

        
        public async Task HandleCustomerCreatedEventAsync(CustomerCreatedEvent evt, CancellationToken stoppingToken)
        {
            using var scope = _scopes.CreateScope();
            var accounts = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            var existing = await accounts.GetByCustomerIdAsync(evt.CustomerId, stoppingToken);
            _logger.LogInformation("Existing account: {Existing}", existing);

            if (existing != null)
            {
                _logger.LogInformation("Account already exists for CustomerId {CustomerId}. Skipping creation.", evt.CustomerId);
                return;
            }

            var account = new Account
            {
                Id = Guid.NewGuid(),
                CustomerId = evt.CustomerId,
                FirstName = evt.FirstName,
                LastName = evt.LastName,
                OpenedAt = DateTime.UtcNow,
                Balance = 0m
            };

            _logger.LogInformation("Creating new account {AccountId} for Customer {CustomerId}", account.Id, account.CustomerId);

            await accounts.AddAsync(account, stoppingToken);
            await accounts.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Account {AccountId} saved to repository.", account.Id);

           
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing CustomerCreatedConsumer resources.");
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
