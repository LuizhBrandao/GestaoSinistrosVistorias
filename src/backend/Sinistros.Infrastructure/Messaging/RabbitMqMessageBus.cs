using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Sinistros.Application.Common;

namespace Sinistros.Infrastructure.Messaging;

public sealed class RabbitMqMessageBus : IMessageBus
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqMessageBus> _logger;

    public RabbitMqMessageBus(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqMessageBus> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string routingKey, CancellationToken cancellationToken = default) where TEvent : class
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(3)
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            var payload = JsonSerializer.SerializeToUtf8Bytes(@event);

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: payload,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Evento {EventType} publicado com sucesso no RabbitMQ (RoutingKey: {RoutingKey}).", typeof(TEvent).Name, routingKey);
        }
        catch (Exception ex)
        {
            // Em ambiente de desenvolvimento desconectado, registra aviso para não interromper a navegação
            _logger.LogWarning(ex, "Não foi possível conectar ao RabbitMQ ({Host}:{Port}). Evento {EventType} retido em log.",
                _options.HostName, _options.Port, typeof(TEvent).Name);
        }
    }
}
