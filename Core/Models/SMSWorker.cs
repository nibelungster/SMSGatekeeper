using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
	public class SMSWorker : ISMSWorker
	{
		public async Task Send(string phoneNumber, string content)
		{
			Console.WriteLine($"Sending from {phoneNumber} content {content}...{DateTime.Now}");
			await Task.Delay(2000);
		}
	}
}
