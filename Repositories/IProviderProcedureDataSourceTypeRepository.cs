using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.Repositories;

public interface IProviderProcedureDataSourceTypeRepository
{
    Task InsertAsync(ProviderProcedureDataSourceType source);
    Task<ProviderProcedureDataSourceType> FetchByNameAsync(string name);
}
