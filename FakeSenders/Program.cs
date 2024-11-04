using System;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Core.Models;

string input = String.Empty;
do
{
	Console.WriteLine(" How many messages you want to send? To quit type exit");
	int messageNumber = 0;
	input = Console.ReadLine();
	bool isInteger = int.TryParse(input, out messageNumber);

	if (!isInteger)
	{
		Console.WriteLine("Please enter integer value.");
	}
	else
	{
		var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
		using (var connection = factory.CreateConnection())
		using (var channel = connection.CreateModel())
		{
			channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Topic);
			channel.QueueDeclare("test1", durable: true, autoDelete: false, exclusive: false);

			for (int i = 0; i < messageNumber; i++)
			{
				var message = new Message() { Sender = $"Sender-{i}", Text = $"TestText-{i}" };
				var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
				var properties = channel.CreateBasicProperties();
				properties.Persistent = true;
				channel.BasicPublish(exchange: "logs", routingKey: "", basicProperties: properties, body: body);
				Console.WriteLine($"[x] Sent {message.Text} from sender {message.Sender}");
			}
		}
	}
} while (!input.Equals("exit", StringComparison.OrdinalIgnoreCase)) ;
