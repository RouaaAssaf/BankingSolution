using Banking.Messaging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text.Json;


public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _conn;
    private readonly IModel _chan;
    private readonly string _exchange;

    public RabbitMqEventPublisher(IConfiguration cfg)
    {
        var uri = cfg["RabbitMq:ConnectionString"] ?? "amqp://guest:guest@localhost:5672/";
        var factory = new ConnectionFactory { Uri = new Uri(uri) };
        factory.DispatchConsumersAsync = true;
        _conn = factory.CreateConnection();
        _chan = _conn.CreateModel();
        _exchange = cfg["RabbitMq:Exchange"] ?? "domain.events";
        _chan.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true);
    }

    public Task PublishAsync<T>(string routingKey, T @event, CancellationToken ct = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(@event);
        var props = _chan.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2;
        _chan.BasicPublish(_exchange, routingKey, props, bytes);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _chan?.Close();
        _conn?.Close();
    }
}