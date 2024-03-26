using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class ProviderProcedureRepository(MediGuruDbContext dbContext) : IProviderProcedureRepository
{
    public async Task<IList<ProviderProcedure>> FetchAll(DateTime? startDate = null)
    {
        if (startDate.HasValue)
        {
            return await dbContext.ProviderProcedures.Where(x => x.DateAdded >= startDate.Value).ToListAsync()
                .ConfigureAwait(false);
        }

        return await dbContext.ProviderProcedures.ToListAsync().ConfigureAwait(false);
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

        var addedEntity = await dbContext.ProviderProcedures.AddAsync(dbProvider).ConfigureAwait(false);
        if (shouldSaveNow)
        {
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        newOne.ProviderProcedureId = addedEntity.Entity.ProviderProcedureId;
    }
}
