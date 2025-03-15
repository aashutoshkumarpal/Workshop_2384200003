using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Middleware.RabbitMQ
{
    public class RabbitMqBackgroundService : BackgroundService
    {
        private readonly RabbitMqConsumer _rabbitMqConsumer;
        private readonly ILogger<RabbitMqBackgroundService> _logger;

        public RabbitMqBackgroundService(RabbitMqConsumer rabbitMqConsumer, ILogger<RabbitMqBackgroundService> logger)
        {
            _rabbitMqConsumer = rabbitMqConsumer;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[RabbitMQ] Consumer Started");
            _rabbitMqConsumer.StartConsuming();
            return Task.CompletedTask;
        }
    }
}
