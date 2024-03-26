using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class DisciplineRepository(MediGuruDbContext dbContext) : IDisciplineRepository
{
    public async Task<Discipline> FetchByCode(string code)
    {
        var disciplines = await dbContext.Disciplines.FirstOrDefaultAsync(x => x.Code == code).ConfigureAwait(false);
        return disciplines;
    }

    public async Task<Discipline> FetchByCodeAndSubCode(string code, string subCode)
    {
        return await dbContext.Disciplines.FirstOrDefaultAsync(x => x.Code == code && x.SubCode == subCode)
            .ConfigureAwait(false);
    }

    public async Task<Discipline?> FetchByName(string name)
    {
        return await dbContext.Disciplines.FirstOrDefaultAsync(x => x.Description == name);
    }

    public async Task<List<Discipline>> FetchAll()
    {
        return await dbContext.Disciplines.ToListAsync();
    }

    public async Task<int> Count()
    {
        return await dbContext.Disciplines.CountAsync().ConfigureAwait(false);
    }

    public async Task InsertAsync(Discipline newOne, bool shouldSaveNow = true)
    {
        var dbDiscipline = new Discipline();

        dbDiscipline.Description = newOne.Description;
        dbDiscipline.Code = newOne.Code;
        dbDiscipline.SubCode = newOne.SubCode;
        dbDiscipline.DateAdded = newOne.DateAdded;

        var addedEntity = await dbContext.Disciplines.AddAsync(dbDiscipline).ConfigureAwait(false);
        if (shouldSaveNow)
        {
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        newOne.DisciplineId = addedEntity.Entity.DisciplineId;
    }
}
