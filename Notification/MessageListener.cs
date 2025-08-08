using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification;

public class MessageListener : IHostedService
{
    private readonly IConnection _connection;
    private readonly IModel _model;
    public MessageListener()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
        _model = _connection.CreateModel();
        _model.ExchangeDeclare("order-exchange", "topic", false, false, null);
        _model.QueueDeclare("order-creation-queue", false, false, false, null);
        _model.QueueBind("order-creation-queue", "order-exchange", "order.created", null);
        _model.QueueBind("order-creation-queue", "order-exchange", "order.failed", null);

    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var consumer = new EventingBasicConsumer(_model);
        consumer.Received += (model, args) =>
        {
            var body = args.Body.ToArray();
            var message = System.Text.Encoding.UTF8.GetString(body);
            var routingKey = args.RoutingKey;
            Console.WriteLine($"Message is recieved - {message} \n routing key - {routingKey}");

        };
        _model.BasicConsume("order-creation-queue", true, consumer);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _connection.Close();
        _model.Close();
    }
}
