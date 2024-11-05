using Core.Interfaces;

namespace Core.Models
{
	public class SMSProcessorFactory : ISMSProcessorFactory
	{
		public ISMSWorker GetSMSWorker()
		{
			return new SMSWorker();
		}
	}
}
