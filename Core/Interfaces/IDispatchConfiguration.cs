using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
	public interface IDispatchConfiguration
	{
		public int LimitPerNumber { get; set; }
		public int LimitPerAccount { get; set; }
		public int ConcurrentConsumers { get; set; }
		public IList<string> PhoneNumbers { get; set; }
	}
}
