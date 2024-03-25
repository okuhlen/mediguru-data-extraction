using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.Repositories;

public interface IEsSearchDataRepository
{
    Task<IList<SearchData>> Fetch(DateTime? startDate = null);
}