using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
	public interface IStatisticRepository
	{
		public Task SaveStatisticAsync(IDictionary<string, int> statiscticDictionary);
	}
}
