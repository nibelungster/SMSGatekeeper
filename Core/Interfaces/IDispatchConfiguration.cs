﻿namespace Core.Interfaces
{
	public interface IDispatchConfiguration
	{
		public int LimitPerNumber { get; set; }
		public int LimitPerAccount { get; set; }
		public int ConcurrentConsumers { get; set; }
		public IList<string> PhoneNumbers { get; set; }
	}
}
