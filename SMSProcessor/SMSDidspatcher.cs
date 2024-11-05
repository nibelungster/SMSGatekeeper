using Core.Interfaces;
using Core.Models;
using Gatekeeper.Core.Interfaces;
using System.Collections.Concurrent;
using System.Security.Principal;

namespace SMSGatekeeper
{
	public class SMSDidspatcher : IDispatcher
	{
		//Probably common dictionary could be ehough but I wanted to play with this one :)
		private ConcurrentDictionary<string, int> _numbersStatisctic;
		private readonly int _limitPerNumber;
		private readonly int _limitPerAccount;
		private ISMSProcessorFactory _factory;
		private IStatisticRepository _statisticRepository;
		private IDispatchConfiguration _dispatchConfiguration;
		static readonly object _collectionLock = new object();

		public SMSDidspatcher(ISMSProcessorFactory factory, IStatisticRepository statisticRepository, IDispatchConfiguration dispatchConfiguration)
		{
			_numbersStatisctic = new ConcurrentDictionary<string, int>();
			_factory = factory;
			_statisticRepository = statisticRepository;
			_dispatchConfiguration = dispatchConfiguration;
			foreach (var number in _dispatchConfiguration.PhoneNumbers)
			{
				_numbersStatisctic.TryAdd(number, 0);
			}
			_limitPerNumber = _dispatchConfiguration.LimitPerNumber;
			_limitPerAccount = _dispatchConfiguration.LimitPerAccount;
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

		public ConcurrentDictionary<string, int> GetDispatcherStatisctic()
		{
			return _numbersStatisctic;
		}

		internal bool IncreasePhoneNumberCount(string phoneNumber)
		{
			int currentValue = 0;
			Console.WriteLine($"Add number {phoneNumber}...{DateTime.Now}");
			lock (_collectionLock)
			{
				if (GetCurrentNumbersInUse == _limitPerAccount)
					return false;

				if (_numbersStatisctic.TryGetValue(phoneNumber, out currentValue) && currentValue == _limitPerNumber)
					return false;

				_numbersStatisctic.AddOrUpdate(
					phoneNumber,
					1,
					(key, oldValue) => oldValue + 1
				);
			}
			return true;
		}

		internal void DecreasePhoneNumberCount(string phoneNumber)
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
				if (!IncreasePhoneNumberCount(phoneNumber))
					return false;
			}
			var message = System.Text.Json.JsonSerializer.Deserialize<Message>(content);
			var smsWorker = _factory.GetSMSWorker();
			Task t = Task.Run(async () => await smsWorker.Send(phoneNumber, content));
			await t.ContinueWith((t1) =>
			{
				Console.WriteLine($"Succesfully sent from {phoneNumber} content {message?.Text} from sender {message?.Sender}...{DateTime.Now}. Status: {t1.Status}");
				DecreasePhoneNumberCount(phoneNumber);
			});
			return true;
		}
	}
}
