using MediGuru.DataExtractionTool.DatabaseModels;

namespace MediGuru.DataExtractionTool.Repositories;

public interface IProviderProcedureRepository
{
    Task InsertAsync(ProviderProcedure newOne, bool shouldSaveNow = true);
    Task<IList<ProviderProcedure>> FetchAll(DateTime? startDate = null);
}
