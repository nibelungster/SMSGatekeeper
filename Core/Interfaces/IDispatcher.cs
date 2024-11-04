using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Core.Interfaces
{
	public interface IDispatcher
	{
		public Task HandleReceivedMessageAsync(string content, string phoneNumber);
		public string GetAvailableNumber();
	}
}
