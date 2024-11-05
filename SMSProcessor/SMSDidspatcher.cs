using Core.Interfaces;
using Core.Models;
using Gatekeeper.Core.Interfaces;
using System.Collections.Concurrent;
using System.Security.Principal;

namespace SMSGatekeeper
{
	public class SMSDidspatcher : IDispatcher
	{
		private readonly List<string> _phoneNumbers = new List<string>() { "+1234", "+4321", "+3333", "+4444", "+6666"};
		private ConcurrentDictionary<string, int> _numbersStatisctic;
		const int _limitPerNumber = 5;
		const int _limitPerAccount = 20;
		private ISMSProcessorFactory _factory;
		private IStatisticRepository _statisticRepository;
		static readonly object _collectionLock = new object();

		public SMSDidspatcher(ISMSProcessorFactory factory, IStatisticRepository statisticRepository)
		{
			_numbersStatisctic = new ConcurrentDictionary<string, int>();
			foreach (var number in _phoneNumbers)
			{
				_numbersStatisctic.TryAdd(number, 0);
			}
			_factory = factory;
			_statisticRepository = statisticRepository;
		}

		public string GetAvailableNumber()
		{
			lock (_collectionLock)
			{
				if (GetCurrentNumbersInUse == _limitPerAccount)
					return null;

				var _availableNumber = _numbersStatisctic.MinBy(x => x.Value);
				if (_availableNumber.Value < _limitPerNumber)
				{
					return _availableNumber.Key;
				}
				else
				{
					return null;
				}
			}
		}

		internal int GetCurrentNumbersInUse
		{
			get { return _numbersStatisctic.Skip(0).Sum(x => x.Value); }
		}

		public ConcurrentDictionary<string, int> GetConcurentDictionaryStats()
		{ 
			return _numbersStatisctic;
		}

		internal bool AddNumberInUse(string phoneNumber)
		{
			int currentValue = 0;
			lock (_collectionLock)
			{
				if (GetCurrentNumbersInUse == _limitPerAccount)
					return false;

				if (_numbersStatisctic.TryGetValue(phoneNumber, out currentValue) && currentValue == _limitPerNumber) 
					return false;

				Console.WriteLine($"Add number {phoneNumber}...{DateTime.Now}");
				_numbersStatisctic.AddOrUpdate(
					phoneNumber,
					1,
					(key, oldValue) => oldValue + 1
				);
			}
			return true;
		}

		internal void RemoveNumberInUse(string phoneNumber)
		{
			Console.WriteLine($"Remove number {phoneNumber}...{DateTime.Now}");
			lock (_collectionLock)
			{
				var currentCount = _numbersStatisctic[phoneNumber];
				_numbersStatisctic.TryUpdate(phoneNumber, currentCount - 1, currentCount);
			}
		}

		public async Task<bool> HandleReceivedMessageAsync(string content, string phoneNumber)
		{
			if (GetCurrentNumbersInUse == _limitPerAccount)
				return false;
			lock (_collectionLock)
			{
				if (GetCurrentNumbersInUse == _limitPerAccount)
					return false;
				if (!AddNumberInUse(phoneNumber))
					return false;
			}
			var message = System.Text.Json.JsonSerializer.Deserialize<Message>(content);
			var smsWorker = _factory.GetSMSWorker();
			Task t = Task.Run(async () => await smsWorker.Send(phoneNumber, content));
			await t.ContinueWith((t1) =>
			{
				Console.WriteLine($"Succesfully sent from {phoneNumber} content {message?.Text} from sender {message?.Sender}...{DateTime.Now}. Status: {t1.Status}");
				RemoveNumberInUse(phoneNumber);
			});
			return true;
		}
	}
}
