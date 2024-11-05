using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using Core.Interfaces;

namespace SMSProcessor.Repositories
{
	public class StatisticReposritory : IStatisticRepository
	{
		public async Task SaveStatisticAsync(IDictionary<string, int> statiscticDictionary)
		{
			String json = JsonConvert.SerializeObject(statiscticDictionary, Newtonsoft.Json.Formatting.Indented);
			using (FileStream file = new FileStream("./Files/Statistic.json", FileMode.Append, FileAccess.Write, FileShare.Read))
			using (StreamWriter writer = new StreamWriter(file, Encoding.Unicode))
			{
				await writer.WriteAsync(json);
			}
		}

		public async Task GetStatisticAsync(IDictionary<string, int> statiscticDictionary)
		{
			String json = JsonConvert.SerializeObject(statiscticDictionary, Newtonsoft.Json.Formatting.Indented);
			using (FileStream file = new FileStream("./Statistic.json", FileMode.Append, FileAccess.Write, FileShare.Read))
			using (StreamWriter writer = new StreamWriter(file, Encoding.Unicode))
			{
				await writer.WriteAsync(json);
			}
		}
	}
}
