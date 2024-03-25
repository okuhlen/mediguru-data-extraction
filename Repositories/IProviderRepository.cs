using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.Repositories;

public interface IProviderRepository
{
    Task InsertAsync(Provider newProvider);

    Task<Provider> FetchByName(string name);

    Task<List<Provider>> FetchAll();
    Task<List<Tuple<string, string, bool>>> FetchDataSourcesByProviderId(string id);

    Task<Provider> FetchById(string id);

    Task<int> Count();

    Task<bool> Exists(string name);

    Task<bool> ExistsById(string id);
}
