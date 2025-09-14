using Banking.Messaging.Events;
using Customers.Api.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;


namespace Customers.Api.Consumers;

public class AccountCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IConfiguration _cfg;
    private IConnection _conn;
    private IModel _chan;
    private readonly string _exchange;

    public AccountCreatedConsumer(IServiceScopeFactory scopes, IConfiguration cfg)
    {
        _scopes = scopes;
        _cfg = cfg;
        _exchange = cfg["RabbitMq:Exchange"] ?? "domain.events";

        var connStr = cfg["RabbitMq:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("RabbitMq:ConnectionString is missing in configuration.");

        var factory = new ConnectionFactory
        {
            Uri = new Uri(connStr),
            DispatchConsumersAsync = true
        };

        _conn = factory.CreateConnection();
        _chan = _conn.CreateModel();
        _chan.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true);
        _chan.QueueDeclare("customer-service.account-created", durable: true, exclusive: false, autoDelete: false);
        _chan.QueueBind("customer-service.account-created", _exchange, "account.created");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_chan);
        consumer.Received += async (s, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evt = JsonSerializer.Deserialize<AccountCreatedEvent>(json);

            if (evt == null)
            {
                _chan.BasicAck(ea.DeliveryTag, false);
                return;
            }

            using var scope = _scopes.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<CustomerAccountsProjectionRepository>();
            await repo.AddAccountAsync(evt.CustomerId, evt.AccountId, evt.InitialBalance, evt.CreatedAt, stoppingToken);

            
            _chan.BasicAck(ea.DeliveryTag, false);
        };

        _chan.BasicConsume("customer-service.account-created", autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _chan?.Close();
        _conn?.Close();
        base.Dispose();
    }
}
