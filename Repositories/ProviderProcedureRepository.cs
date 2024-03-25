using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class ProviderProcedureRepository : IProviderProcedureRepository
{
    private readonly MediGuruDbContext _dbContext;
    public ProviderProcedureRepository(MediGuruDbContext dbContext) => _dbContext = dbContext;

    public async Task<int> CountProceduresByProviderId(string providerId)
    {
        return await _dbContext.ProviderProcedures.CountAsync(x => x.ProviderId == providerId).ConfigureAwait(false);
    }

    public async Task<IList<ProviderProcedure>> FetchAll(DateTime? startDate = null)
    {
        if (startDate.HasValue)
        {
            return await _dbContext.ProviderProcedures.Where(x => x.DateAdded >= startDate.Value).ToListAsync()
                .ConfigureAwait(false);
        }

        return await _dbContext.ProviderProcedures.ToListAsync().ConfigureAwait(false);
    }

    public async Task InsertAsync(ProviderProcedure newOne, bool shouldSaveNow = true)
    {
        var dbProvider = new ProviderProcedure
        {
            NonPayable = newOne.NonPayable,
            Price = newOne.Price,
            DisciplineId = newOne.DisciplineId,
            ProviderId = newOne.ProviderId,
            ProviderProcedureTypeId = newOne.ProviderProcedureTypeId,
            ProcedureId = newOne.ProcedureId,
            ProviderProcedureDataSourceTypeId = newOne.ProviderProcedureDataSourceTypeId,
            AdditionalNotes = newOne.AdditionalNotes,
            DateAdded = DateTime.Now,
            YearValidFor = newOne.YearValidFor,
            IsContracted = newOne.IsContracted,
            IsNonContracted = newOne.IsNonContracted,
        };

        var addedEntity = await _dbContext.ProviderProcedures.AddAsync(dbProvider).ConfigureAwait(false);
        if (shouldSaveNow)
        {
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        newOne.ProviderProcedureId = addedEntity.Entity.ProviderProcedureId;
    }
}
