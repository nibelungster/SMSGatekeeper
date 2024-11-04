using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.AspNetCore.Connections;
using Gatekeeper.Core.Interfaces;

namespace SMSGatekeeper
{
	public class SMSQueueConsumerService : BackgroundService
	{
		private readonly ILogger<SMSQueueConsumerService> _logger;
		private ConnectionFactory _connectionFactory;
		private IConnection _connection;
		private IModel _channel;
		private IDispatcher _dispatcher;
		private string _queueName;
		public IServiceProvider Services { get; }

		public SMSQueueConsumerService(ILoggerFactory loggerFactory, IServiceProvider service)
		{
			Services = service;
			using (var scope = Services.CreateScope())
			{
				_dispatcher =
					scope.ServiceProvider
						.GetRequiredService<IDispatcher>();
			}
			_logger = loggerFactory.CreateLogger<SMSQueueConsumerService>();
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			var factory = new ConnectionFactory()
			{
				HostName = "localhost",
				UserName = "guest",
				Password = "guest",
				DispatchConsumersAsync = true
			};
			_connection = factory.CreateConnection();

			_logger.LogInformation($"Queue [{_queueName}] is waiting for messages.");

			return base.StartAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			for (int i = 1; i < 20; i++)
			{
				var channel = _connection.CreateModel();

				channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Topic);
				var queueName = channel.QueueDeclare("test1", durable: true, autoDelete: false, exclusive: false);

				// take 1 message per consumer
				channel.BasicQos(0, 1, false);

				channel.QueueBind(queue: queueName,
					exchange: "logs",
					routingKey: "");

				var consumer = new AsyncEventingBasicConsumer(channel);
				consumer.Received += async (ch, message) =>
					{
						// received body
						var content = Encoding.UTF8.GetString(message.Body.ToArray());
						var phoneNumber = _dispatcher.GetAvailableNumber();
						if (String.IsNullOrEmpty(phoneNumber))
						{
							Console.WriteLine($"[-] CANT PROCESS {content} consumer! No available numbers.");
							channel.BasicNack(deliveryTag: message.DeliveryTag, multiple: false, true);
						}
						else
						{
							await HandleReceivedMessageAsync(content, phoneNumber);
							channel.BasicAck(deliveryTag: message.DeliveryTag, multiple: false);
						}
					};

				channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
			}
			await Task.CompletedTask;
		}

		public override void Dispose()
		{
			//_channel.Dispose();
			_connection.Close();
			base.Dispose();
		}

		private async Task HandleReceivedMessageAsync(string content, string phoneNumber)
		{
			await _dispatcher.HandleReceivedMessageAsync(content, phoneNumber);
		}
	}
}