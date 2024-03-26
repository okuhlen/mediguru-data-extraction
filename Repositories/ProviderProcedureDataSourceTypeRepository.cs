using MediGuru.DataExtractionTool.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class ProviderProcedureDataSourceTypeRepository(MediGuruDbContext dbContext)
    : IProviderProcedureDataSourceTypeRepository
{
    public async Task<ProviderProcedureDataSourceType> FetchByNameAsync(string name)
    {
        return await dbContext.ProviderProcedureDataSourceTypes.FirstAsync(x => x.Name == name) ??
            throw new Exception($"Could not find source type: {name}");
    }

    public async Task InsertAsync(ProviderProcedureDataSourceType source)
    {
        var newType = new ProviderProcedureDataSourceType()
        {
            Name = source.Name,
            SourceUrl = source.SourceUrl,
            WebsiteUrl = source.WebsiteUrl
        };

        var result = await dbContext.ProviderProcedureDataSourceTypes.AddAsync(newType);
        await dbContext.SaveChangesAsync();

        source.ProviderProcedureDataSourceTypeId = result.Entity.ProviderProcedureDataSourceTypeId;
    }
}
