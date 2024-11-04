using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
