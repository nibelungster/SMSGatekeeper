using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
	public interface ISMSWorker
	{
		public Task Send(string phoneNumber, string content);
	}
}
