using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class DisciplineRepository : IDisciplineRepository
{
    private readonly MediGuruDbContext _dbContext;
    public DisciplineRepository(MediGuruDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Discipline> FetchByCode(string code)
    {
        var disciplines = await _dbContext.Disciplines.FirstOrDefaultAsync(x => x.Code == code).ConfigureAwait(false);
        return disciplines;
    }

    public async Task<Discipline> FetchByCodeAndSubCode(string code, string subCode)
    {
        return await _dbContext.Disciplines.FirstOrDefaultAsync(x => x.Code == code && x.SubCode == subCode)
            .ConfigureAwait(false);
    }

    public async Task<bool> Exists(string code)
    {
        return await _dbContext.Disciplines.AnyAsync(x => x.Code == code);
    }

    public async Task<bool> Exists(string code, string subCode)
    {
        return await _dbContext.Disciplines.AnyAsync(x => x.Code == code && x.SubCode == subCode);
    }

    public async Task<Discipline?> FetchByName(string name)
    {
        return await _dbContext.Disciplines.FirstOrDefaultAsync(x => x.Description == name);
    }

    public async Task<List<Discipline>> FetchAll()
    {
        return await _dbContext.Disciplines.ToListAsync();
    }

    public async Task<Discipline> FetchById(string id)
    {
        return await _dbContext.Disciplines.FirstAsync(x => x.DisciplineId == id) ??
               throw new Exception($"Could not find discipline by id: {id}");
    }

    public async Task<int> Count()
    {
        return await _dbContext.Disciplines.CountAsync().ConfigureAwait(false);
    }

    public async Task InsertAsync(Discipline newOne, bool shouldSaveNow = true)
    {
        var dbDiscipline = new Discipline();

        dbDiscipline.Description = newOne.Description;
        dbDiscipline.Code = newOne.Code;
        dbDiscipline.SubCode = newOne.SubCode;
        dbDiscipline.DateAdded = newOne.DateAdded;

        var addedEntity = await _dbContext.Disciplines.AddAsync(dbDiscipline).ConfigureAwait(false);
        if (shouldSaveNow)
        {
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        newOne.DisciplineId = addedEntity.Entity.DisciplineId;
    }
}
