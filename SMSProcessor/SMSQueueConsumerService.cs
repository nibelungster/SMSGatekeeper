using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.AspNetCore.Connections;
using Gatekeeper.Core.Interfaces;
using Core.Interfaces;
using Core.Models;

namespace SMSGatekeeper
{
	public class SMSQueueConsumerService : BackgroundService
	{
		private readonly ILogger<SMSQueueConsumerService> _logger;
		private ConnectionFactory _connectionFactory;
		private IConnection _connection;
		private IModel _channel;
		private IDispatcher _dispatcher;
		private IStatisticRepository _statisticRepository;
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
				_statisticRepository =
					scope.ServiceProvider
						.GetRequiredService<IStatisticRepository>();
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
						var content = Encoding.UTF8.GetString(message.Body.ToArray());
						var phoneNumber = _dispatcher.GetAvailableNumber();
						if (String.IsNullOrEmpty(phoneNumber))
						{
							await MoveBackToQueue(channel, message, content);
						}
						else
						{
							if (await HandleReceivedMessageAsync(content, phoneNumber))
							{
								channel.BasicAck(deliveryTag: message.DeliveryTag, multiple: false);
							}
							else
							{
								await MoveBackToQueue(channel, message, content);
							}
						}
					};

				channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
			}
			await Task.CompletedTask;
		}

		private async Task MoveBackToQueue(IModel channel, BasicDeliverEventArgs message, string content)
		{
			Console.WriteLine($"[-] CANT PROCESS {content} consumer! No available numbers.");
			channel.BasicNack(deliveryTag: message.DeliveryTag, multiple: false, true);
			await _statisticRepository.SaveStatisticAsync(_dispatcher.GetConcurentDictionaryStats());
		}

		public override void Dispose()
		{
			//_channel.Dispose();
			_connection.Close();
			base.Dispose();
		}

		private async Task<bool> HandleReceivedMessageAsync(string content, string phoneNumber)
		{
			return await _dispatcher.HandleReceivedMessageAsync(content, phoneNumber);
		}
	}
}