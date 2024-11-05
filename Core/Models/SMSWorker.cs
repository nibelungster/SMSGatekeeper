using Core.Interfaces;

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
