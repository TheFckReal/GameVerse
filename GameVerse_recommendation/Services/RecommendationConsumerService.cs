using System.Text;
using System.Text.Json;
using System.Threading;
using GameVerse_recommendation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GameVerse_recommendation.Services
{
    public class RecommendationConsumerService : IHostedService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RecommendationConsumerService> _logger;
        private readonly RecommendationsCache _cache;
        private readonly IServiceScopeFactory _serviceFactory;
        private AsyncEventingBasicConsumer _consumer;


        private readonly string _queueName;
        public RecommendationConsumerService(IConnectionFactory connectionFactory, IConfiguration configuration, 
            ILogger<RecommendationConsumerService> logger, RecommendationsCache cache, IServiceScopeFactory factoryFactory)
        {
            _logger = logger;
            _cache = cache;
            _serviceFactory = factoryFactory;

            string? exchangeName = configuration.GetValue<string>("Rabbit:Exchange");
            ArgumentException.ThrowIfNullOrEmpty(argument: exchangeName, paramName: nameof(exchangeName));

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);

            _queueName = _channel.QueueDeclare(exclusive:false, autoDelete:true).QueueName;
            _channel.QueueBind(_queueName, exchangeName, String.Empty);
            _logger.LogCritical("Exchange name {0}", exchangeName);

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DoWorkSync(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task DoWorkSync(CancellationToken cancellationToken)
        {
            _consumer = new AsyncEventingBasicConsumer(_channel);
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Cancellation was requested");
                return;
            }
            _consumer.Received += MessageReceive;
            _channel.BasicConsume(queue: _queueName,
                autoAck: false,
                consumer: _consumer);
            _logger.LogInformation("Rabbit consumer is up with queue name {0}", _queueName);
        }
        private async Task MessageReceive(object? sender, BasicDeliverEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Body.Span);
            Dictionary<string, List<int>>? playeridRecommendations = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(message);

            if (playeridRecommendations is null)
            {
                _logger.LogCritical("Received message from RabbitMQ was not deserializable");
                return;
            }

            string? playerId = playeridRecommendations.First().Key;

            using (var scope = _serviceFactory.CreateScope())
            {
                UserManager<Player> userManager = scope.ServiceProvider.GetService<UserManager<Player>>() ?? throw new ArgumentNullException($"Service {nameof(UserManager<Player>)} was not found");
                var player = await userManager.FindByIdAsync(playerId);
                if (player is null)
                {
                    _logger.LogWarning("Has not found user with Id {0}", playerId);
                    return;
                }
            }
            
            var playersRecommendations = playeridRecommendations[playerId];
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetSize(1);
            _cache.Set(playerId, playersRecommendations, cacheEntryOptions);

            _logger.LogInformation("Recommendations ids for playerId {0} have been set", playerId);
        }



        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rabbit consumer is down with queue name {0}", _queueName);
            return Task.CompletedTask;
        }


        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
            _connection.Close();
            _channel.Close();

        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~RecommendationConsumerService()
        {
            ReleaseUnmanagedResources();
        }
    }
}
