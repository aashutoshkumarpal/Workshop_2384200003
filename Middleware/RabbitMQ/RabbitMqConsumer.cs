using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Middleware.Email;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Middleware.RabbitMQ
{
    public class RabbitMqConsumer
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly string _routingKey;
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMqConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _hostName = configuration["RabbitMQ:HostName"];
            _userName = configuration["RabbitMQ:UserName"];
            _password = configuration["RabbitMQ:Password"];
            _exchangeName = configuration["RabbitMQ:ExchangeName"];
            _queueName = configuration["RabbitMQ:QueueName"];
            _routingKey = configuration["RabbitMQ:RoutingKey"];
            _serviceProvider = serviceProvider;
        }

        public void StartConsuming()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
            _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(_queueName, _exchangeName, _routingKey);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"[Consumer] Received: {message}");

                // Send Email using EmailService
                using (var scope = _serviceProvider.CreateScope())
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                    emailService.SendEmail("aashutoshkumarpal02@gmail.com", "RabbitMQ Event Notification", message);
                }

                // ✅ Acknowledge the message after processing
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            // ✅ Set autoAck to false to prevent message loss
            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

            Console.WriteLine("[Consumer] Listening for messages...");
        }
    }
}
