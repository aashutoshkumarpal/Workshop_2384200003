using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Text;

namespace Middleware.RabbitMQ
{
    public class RabbitMqService 
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly string _routingKey;

        public RabbitMqService(IConfiguration configuration)
        {
            _hostName = configuration["RabbitMQ:HostName"];
            _userName = configuration["RabbitMQ:UserName"];
            _password = configuration["RabbitMQ:Password"];
            _exchangeName = configuration["RabbitMQ:ExchangeName"];
            _queueName = configuration["RabbitMQ:QueueName"];
            _routingKey = configuration["RabbitMQ:RoutingKey"];
        }

        public void PublishMessage(string message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
                channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind(_queueName, _exchangeName, _routingKey);

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: _exchangeName, routingKey: _routingKey, basicProperties: null, body: body);

                Console.WriteLine($"[Publisher] Sent: {message}");
            }
        }
    }
}
