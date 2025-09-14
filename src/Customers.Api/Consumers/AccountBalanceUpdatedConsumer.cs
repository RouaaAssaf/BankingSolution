using Banking.Messaging.Events;
using Customers.Api.Repositories;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Customers.Api.Consumers;

public class AccountBalanceUpdatedConsumer : BackgroundService
{
    private readonly IConfiguration _cfg;
    private readonly IServiceScopeFactory _scopes;
    private IConnection _conn;
    private IModel _chan;
    private readonly string _exchange;

    public AccountBalanceUpdatedConsumer(IServiceScopeFactory scopes, IConfiguration cfg)
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
        _chan.QueueDeclare("customer-service.account-balance-updated", durable: true, exclusive: false, autoDelete: false);
        _chan.QueueBind("customer-service.account-balance-updated", _exchange, "account.balance.updated");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_chan);

        consumer.Received += async (s, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<AccountBalanceUpdatedEvent>(json);

                // Check for null to avoid warnings
                if (evt == null)
                {
                    _chan.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                using var scope = _scopes.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<CustomerAccountsProjectionRepository>();

                // Update the balance safely
                await repo.UpdateBalanceAsync(evt.CustomerId, evt.AccountId, evt.NewBalance, stoppingToken);

                _chan.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                // log exception if you have logging
                Console.WriteLine($"Error in AccountBalanceUpdatedConsumer: {ex.Message}");
                // optionally, you can nack/requeue the message
                _chan.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _chan.BasicConsume("customer-service.account-balance-updated", autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _chan?.Close();
        _conn?.Close();
        base.Dispose();
    }
}
