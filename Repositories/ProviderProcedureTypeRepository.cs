using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class ProviderProcedureTypeRepository : IProviderProcedureTypeRepository
{
    private readonly MediGuruDbContext _dbContext;

    public ProviderProcedureTypeRepository(MediGuruDbContext dbContext) => _dbContext = dbContext;
    
    public async Task InsertAsync(ProviderProcedureType newType)
    {
        var dbType = new ProviderProcedureType
        {
            Name = newType.Name
        };
        var addResult = await _dbContext.ProviderProcedureTypes.AddAsync(dbType);
        await _dbContext.SaveChangesAsync();
        newType.ProviderProcedureTypeId = addResult.Entity.ProviderProcedureTypeId;
    }

    public async Task<ProviderProcedureType> FetchByName(string name)
        => await _dbContext.ProviderProcedureTypes.FirstOrDefaultAsync(x => x.Name == name);
}