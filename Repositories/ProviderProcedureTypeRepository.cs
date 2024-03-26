using MediGuru.DataExtractionTool.DatabaseModels;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class ProviderProcedureTypeRepository(MediGuruDbContext dbContext) : IProviderProcedureTypeRepository
{
    public async Task InsertAsync(ProviderProcedureType newType)
    {
        var dbType = new ProviderProcedureType
        {
            Name = newType.Name
        };
        var addResult = await dbContext.ProviderProcedureTypes.AddAsync(dbType);
        await dbContext.SaveChangesAsync();
        newType.ProviderProcedureTypeId = addResult.Entity.ProviderProcedureTypeId;
    }
}