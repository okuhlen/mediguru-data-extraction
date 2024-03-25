using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class ProviderProcedureDataSourceTypeRepository : IProviderProcedureDataSourceTypeRepository
{
    private readonly MediGuruDbContext _dbContext;

    public ProviderProcedureDataSourceTypeRepository(MediGuruDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProviderProcedureDataSourceType> FetchByNameAsync(string name)
    {
        return await _dbContext.ProviderProcedureDataSourceTypes.FirstAsync(x => x.Name == name) ??
            throw new Exception($"Could not find source type: {name}");
    }

    public async Task<bool> ExistsById(string id)
    {
        return await _dbContext.ProviderProcedureDataSourceTypes
            .AnyAsync(x => x.ProviderProcedureDataSourceTypeId == id).ConfigureAwait(false);
    }

    public async Task InsertAsync(ProviderProcedureDataSourceType source)
    {
        var newType = new ProviderProcedureDataSourceType()
        {
            Name = source.Name,
            SourceUrl = source.SourceUrl,
            WebsiteUrl = source.WebsiteUrl
        };

        var result = await _dbContext.ProviderProcedureDataSourceTypes.AddAsync(newType);
        await _dbContext.SaveChangesAsync();

        source.ProviderProcedureDataSourceTypeId = result.Entity.ProviderProcedureDataSourceTypeId;
    }
}
