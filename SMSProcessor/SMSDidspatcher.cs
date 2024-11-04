using Core.Interfaces;
using Core.Models;
using Gatekeeper.Core.Interfaces;
using System.Collections.Concurrent;

namespace SMSGatekeeper
{
	public class SMSDidspatcher : IDispatcher
	{
		private readonly List<string> _phoneNumbers = new List<string>() { "+1234", "+4321", "+3333", "+555"};
		private ConcurrentDictionary<string, int> _numbersStatisctic;
		const int _limitPerNumber = 5;
		const int _limitPerAccount = 15;
		private ISMSProcessorFactory _factory;

		public SMSDidspatcher(ISMSProcessorFactory factory)
		{
			_numbersStatisctic = new ConcurrentDictionary<string, int>();
			foreach (var number in _phoneNumbers)
			{
				_numbersStatisctic.TryAdd(number, 0);
			}
			_factory = factory;
		}

		public string GetAvailableNumber()
		{
			if (GetCurrentNumbersInUse == _limitPerAccount)
				return null;

			var _availableNumber = _numbersStatisctic.FirstOrDefault(x => x.Value < _limitPerNumber);
			return _availableNumber.Key;
		}

		internal int GetCurrentNumbersInUse
		{
			get { return _numbersStatisctic.Skip(0).Sum(x => x.Value); }
		}

		internal void AddNumberInUse(string phoneNumber)
		{
			Console.WriteLine($"Add number {phoneNumber}...{DateTime.Now}");
			_numbersStatisctic.AddOrUpdate(
				phoneNumber,
				1,
				(key, oldValue) => oldValue + 1
			);
		}

		internal void RemoveNumberInUse(string phoneNumber)
		{
			Console.WriteLine($"Remove number {phoneNumber}...{DateTime.Now}");
			var currentCount = _numbersStatisctic[phoneNumber];
			_numbersStatisctic.TryUpdate(phoneNumber, currentCount - 1, currentCount);
		}

		public async Task HandleReceivedMessageAsync(string content, string phoneNumber)
		{
			AddNumberInUse(phoneNumber);
			var message = System.Text.Json.JsonSerializer.Deserialize<Message>(content);
			var smsWorker = _factory.GetSMSWorker();
			Task t = Task.Run(async () => await smsWorker.Send(phoneNumber, content));
			await t.ContinueWith((t1) =>
			{
				Console.WriteLine($"Succesfully sent from {phoneNumber} content {message?.Text} from sender {message?.Sender}...{DateTime.Now}. Status: {t1.Status}");
				RemoveNumberInUse(phoneNumber);
			});
		}

		//private async Task SendSMSEmulationAsync(string phoneNumber, string content)
		//{
		//	Console.WriteLine($"Sending from {phoneNumber} content {content}...{DateTime.Now}");
		//	await Task.Delay(3000);
		//}
	}
}
