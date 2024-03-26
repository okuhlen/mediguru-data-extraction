using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class ProviderRepository(MediGuruDbContext dbContext) : IProviderRepository
{
    public async Task<Provider> FetchByName(string name)
    {
        return await dbContext.Providers.FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<List<Provider>> FetchAll()
    {
        return await dbContext.Providers.ToListAsync();
    }

    public async Task InsertAsync(Provider newProvider)
    {
        var dbProvider = new Provider();
        dbProvider.Name = newProvider.Name;
        dbProvider.Description = newProvider.Description;
        dbProvider.AddedDate = newProvider.AddedDate;
        dbProvider.WebsiteUrl = newProvider.WebsiteUrl;
        dbProvider.DisplayPictureReferenceId = newProvider.DisplayPictureReferenceId;

        var addedEntity = await dbContext.Providers.AddAsync(dbProvider);
        await dbContext.SaveChangesAsync();

        newProvider.ProviderId = addedEntity.Entity.ProviderId;
    }
    
}
