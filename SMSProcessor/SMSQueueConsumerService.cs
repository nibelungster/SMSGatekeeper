using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Gatekeeper.Core.Interfaces;
using Core.Interfaces;

namespace SMSGatekeeper
{
	public class SMSQueueConsumerService : BackgroundService
	{
		private readonly ILogger<SMSQueueConsumerService> _logger;
		private readonly IDispatchConfiguration _configuration;
		private ConnectionFactory _connectionFactory;
		private IConnection _connection;
		private IDispatcher _dispatcher;
		private IStatisticRepository _statisticRepository;
		public IServiceProvider Services { get; }


		public SMSQueueConsumerService(ILoggerFactory loggerFactory, IServiceProvider service, IDispatchConfiguration configuration)
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
			_configuration = configuration;
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

			return base.StartAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			//It is simplest probably wey to scale the application - just increse several async consumers.
			//Also there is additional option just to ad new queue and make a half of consumers listen
			//first one and a second half - listen second one. And don;t forget to change ExchangeType from Topic!
			//Another ways how to scale application I will describe in README file. 
			for (int i = 1; i < _configuration.ConcurrentConsumers; i++)
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

		/// <summary>
		/// Method moves message back to queue
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="message"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		private async Task MoveBackToQueue(IModel channel, BasicDeliverEventArgs message, string content)
		{
			///We do it if there is no free available number to send the message.
			Console.WriteLine($"[-] CANT PROCESS {content} consumer! No available numbers.");
			channel.BasicNack(deliveryTag: message.DeliveryTag, multiple: false, true);
			await _statisticRepository.SaveStatisticAsync(_dispatcher.GetDispatcherStatisctic());
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