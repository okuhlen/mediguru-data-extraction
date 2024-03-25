using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public class ProcedureRepository : IProcedureRepository
{
    private readonly MediGuruDbContext _dbContext;

    public ProcedureRepository(MediGuruDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Procedure?> FindByCodeAsync(string code)
    {
        var result = await _dbContext.Procedures.FirstOrDefaultAsync(x => x.Code == code);
        return result;
    }

    public async Task<bool> ExistsByCodeAndCategoryId(string tariffCode, string categoryId) 
        =>  await _dbContext.Procedures.AnyAsync(x => x.Code == tariffCode && x.CategoryId == categoryId).ConfigureAwait(false);

    public async Task<Procedure?> FetchByCodeAndCategoryId(string tariffCode, string categoryId) 
        => await _dbContext.Procedures.FirstOrDefaultAsync(x => x.Code == tariffCode && x.CategoryId == categoryId).ConfigureAwait(false);

    public async Task<int> Count()
    {
        return await _dbContext.Procedures.CountAsync().ConfigureAwait(false);
    }

    public async Task<IList<Procedure>> FetchAll()
    {
        return await _dbContext.Procedures.ToListAsync().ConfigureAwait(false);
    }

    public async Task<bool> ExistsById(string id)
    {
        return await _dbContext.Procedures.AnyAsync(x => x.ProcedureId == id).ConfigureAwait(false);
    }

    public async Task<Procedure> FetchById(string id)
    {
        return await _dbContext.Procedures.SingleAsync(x => x.ProcedureId == id).ConfigureAwait(false);
    }

    public async Task<IList<Procedure>> FetchByCategoryId(string categoryId)
    {
        return await _dbContext.Procedures.OrderBy(x => x.Code).Where(x => x.CategoryId == categoryId).ToListAsync().ConfigureAwait(false);
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

            var result = await _dbContext.Procedures.AddAsync(dbProcedure).ConfigureAwait(false);
            if (shouldSaveNow)
            {
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            newOne.ProcedureId = result.Entity.ProcedureId;
    }
}
