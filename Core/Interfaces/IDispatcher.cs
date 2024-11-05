using System.Collections.Concurrent;

namespace Gatekeeper.Core.Interfaces
{
	public interface IDispatcher
	{
		public Task<bool> HandleReceivedMessageAsync(string content, string phoneNumber);
		public string GetAvailableNumber();

		public ConcurrentDictionary<string, int> GetDispatcherStatisctic();
	}
}
