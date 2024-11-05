namespace Core.Interfaces
{
	public interface IStatisticRepository
	{
		public Task SaveStatisticAsync(IDictionary<string, int> statiscticDictionary);
	}
}
