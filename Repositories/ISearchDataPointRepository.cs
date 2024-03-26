namespace MediGuru.DataExtractionTool.Repositories;

public interface ISearchDataPointRepository
{
    Task<bool> IsEmpty();
}