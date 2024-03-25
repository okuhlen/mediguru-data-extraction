namespace MediGuru.DataExtractionTool.Repositories;

public interface ISearchDataRepository
{
    Task<bool> IsEmpty();
}