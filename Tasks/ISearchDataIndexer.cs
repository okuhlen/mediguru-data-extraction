namespace MediGuru.DataExtractionTool.Tasks;

public interface ISearchDataIndexer
{
    Task UpdateIndex(DateTime? startDate = null);
}