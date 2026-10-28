namespace Sinistros.Application.Common;

/// <summary>
/// Interface desacoplada para publicação de eventos assíncronos no broker de mensageria (RabbitMQ).
/// </summary>
public interface IMessageBus
{
    Task PublishAsync<TEvent>(TEvent @event, string routingKey, CancellationToken cancellationToken = default) where TEvent : class;
}
