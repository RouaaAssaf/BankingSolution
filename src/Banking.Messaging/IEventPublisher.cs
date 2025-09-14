
namespace Banking.Messaging;
public interface IEventPublisher
{
    Task PublishAsync<T>(string routingKey, T @event, CancellationToken ct = default);
}