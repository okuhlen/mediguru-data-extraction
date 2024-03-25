using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.Tasks;

public interface IProviderProcedureDataPointsRetriever
{
    Task<ILookup<string, SearchDataPointModel>> FetchAllDataPoints(DateTime? startDate);
}