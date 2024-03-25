using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class ProviderRepository : IProviderRepository
{
    private readonly MediGuruDbContext _dbContext;
    public ProviderRepository(MediGuruDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Provider> FetchByName(string name)
    {
        return await _dbContext.Providers.FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<List<Tuple<string, string, bool>>> FetchDataSourcesByProviderId(string id)
    {
        var query = from providers in _dbContext.Providers
                    join providerProcedures in _dbContext.ProviderProcedures on providers.ProviderId equals providerProcedures.ProviderId
                    join sources in _dbContext.ProviderProcedureDataSourceTypes on providerProcedures.ProviderProcedureDataSourceTypeId equals sources.ProviderProcedureDataSourceTypeId
                    where providers.ProviderId == id
                    select new
                    {
                        SourceId = sources.ProviderProcedureDataSourceTypeId,
                        SourceName = sources.Name,
                        Url = sources.SourceUrl,
                        IsOfficial = sources.IsOfficialSource
                    };

        var results = await query.ToListAsync();
        return results
            .DistinctBy(key => new { key.SourceId, key.SourceName })
            .Select(x => new Tuple<string, string, bool>(x.SourceName, x.Url, x.IsOfficial))
            .ToList();
    }

    public async Task<Provider> FetchById(string id)
    {
        return await _dbContext.Providers.FirstAsync(x => x.ProviderId == id)
               ?? throw new Exception($"Provider with id: {id} not found");
    }

    public async Task<int> Count()
    {
        return await _dbContext.Providers.CountAsync().ConfigureAwait(false);
    }

    public async Task<bool> Exists(string name)
    {
        return await _dbContext.Providers.AnyAsync(x => x.Name == name).ConfigureAwait(false);
    }

    public async Task<bool> ExistsById(string id)
    {
        return await _dbContext.Providers.AnyAsync(x => x.ProviderId == id).ConfigureAwait(false);
    }

    public async Task<List<Provider>> FetchAll()
    {
        return await _dbContext.Providers.ToListAsync();
    }

    public async Task InsertAsync(Provider newProvider)
    {
        var dbProvider = new Provider();
        dbProvider.Name = newProvider.Name;
        dbProvider.Description = newProvider.Description;
        dbProvider.AddedDate = newProvider.AddedDate;
        dbProvider.WebsiteUrl = newProvider.WebsiteUrl;
        dbProvider.DisplayPictureReferenceId = newProvider.DisplayPictureReferenceId;

        var addedEntity = await _dbContext.Providers.AddAsync(dbProvider);
        await _dbContext.SaveChangesAsync();

        newProvider.ProviderId = addedEntity.Entity.ProviderId;
    }
    
}
