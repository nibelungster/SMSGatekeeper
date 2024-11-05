using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
	public class DispatchConfiguration : IDispatchConfiguration
	{
		public int LimitPerNumber { get; set; }
		public int LimitPerAccount { get; set; }
		public int ConcurrentConsumers { get; set; }
		public IList<string> PhoneNumbers { get; set; }
	}
}
