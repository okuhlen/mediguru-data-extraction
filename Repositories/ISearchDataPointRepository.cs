using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.Repositories;

public interface ISearchDataPointRepository
{
    Task Insert(SearchDataPoint searchDataPoint);
    Task<IList<SearchDataPoint>> FetchBySearchDataId(string searchDataId);
    Task<bool> IsEmpty();
}