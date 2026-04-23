using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderFlow.CQRS.Infrastructure.Messaging;

public class OrderProcessingConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<OrderProcessingConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    private const string QueueName = "order-processing";

    public OrderProcessingConsumer(
        IServiceProvider serviceProvider,
        IOptions<RabbitMqSettings> settings,
        ILogger<OrderProcessingConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Processing Consumer starting...");

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    _logger.LogInformation("Received order processing message: {Message}", message);

                    using var scope = _serviceProvider.CreateScope();
                    await ProcessOrderAsync(message, stoppingToken);

                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
                    _logger.LogInformation("Order processing message acknowledged");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order message: {Message}", message);
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation("Order Processing Consumer started, listening on queue: {QueueName}", QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Order Processing Consumer stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Order Processing Consumer encountered an error");
        }
    }

    private Task ProcessOrderAsync(string message, CancellationToken cancellationToken)
    {
        var orderData = JsonSerializer.Deserialize<JsonElement>(message);
        _logger.LogInformation("Processing order: {OrderData}", orderData);

        // Burada sipariş işleme mantığı uygulanabilir:
        // - Stok kontrolü
        // - Ödeme işleme
        // - Fatura oluşturma
        // - Bildirim gönderme

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
            await _channel.CloseAsync(cancellationToken);

        if (_connection is { IsOpen: true })
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
