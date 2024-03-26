using MediGuru.DataExtractionTool.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public class ProcedureRepository(MediGuruDbContext dbContext) : IProcedureRepository
{
    public async Task<bool> ExistsByCodeAndCategoryId(string tariffCode, string categoryId) 
        =>  await dbContext.Procedures.AnyAsync(x => x.Code == tariffCode && x.CategoryId == categoryId).ConfigureAwait(false);

    public async Task<Procedure?> FetchByCodeAndCategoryId(string tariffCode, string categoryId) 
        => await dbContext.Procedures.FirstOrDefaultAsync(x => x.Code == tariffCode && x.CategoryId == categoryId).ConfigureAwait(false);

    public async Task<IList<Procedure>> FetchAll()
    {
        return await dbContext.Procedures.ToListAsync().ConfigureAwait(false);
    }

    public async Task InsertAsync(Procedure newOne, bool shouldSaveNow = true)
    {
            var dbProcedure = new Procedure
            {
                CodeDescriptor = newOne.CodeDescriptor,
                CreatedDate = newOne.CreatedDate,
                CategoryId = newOne.CategoryId,
                Category = newOne.Category,
                Code = newOne.Code,
            };

            var result = await dbContext.Procedures.AddAsync(dbProcedure).ConfigureAwait(false);
            if (shouldSaveNow)
            {
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            newOne.ProcedureId = result.Entity.ProcedureId;
    }
}
