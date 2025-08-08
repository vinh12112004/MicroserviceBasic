using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using System.Text.Json;

namespace OrderAPI
{
    public class MessageBroker
    {
        private readonly IConnection _connection;
        private readonly IModel _model;
        public MessageBroker()
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
        }
        public void SendMessage(string message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = System.Text.Encoding.UTF8.GetBytes(json);
            var basicProperties = _model.CreateBasicProperties();
            basicProperties.ContentType= "application/json";
            _model.BasicPublish("order-exchange", "order.created", basicProperties, body);
        }
    }
}
