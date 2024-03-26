using MediGuru.DataExtractionTool.DatabaseModels;

namespace MediGuru.DataExtractionTool.Repositories;

public interface IProviderRepository
{
    Task InsertAsync(Provider newProvider);

    Task<Provider> FetchByName(string name);

    Task<List<Provider>> FetchAll();
}
