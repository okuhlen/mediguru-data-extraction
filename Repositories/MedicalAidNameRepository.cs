using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class MedicalAidNameRepository(MediGuruDbContext dbContext) : IMedicalAidNameRepository
{
    public async Task<List<MedicalAidNameDetail>> FetchAll()
    {
        var names = await dbContext.MedicalAidSchemes.ToListAsync();
        return names.ConvertAll(x => new MedicalAidNameDetail
        {
            Name = x.Name,
            Id = x.MedicalAidSchemeId
        });
    }
    
    public async Task InsertBulk(List<string> medicalAidNames)
    {
        var now = DateTime.Now;
        await dbContext.MedicalAidSchemes.AddRangeAsync(medicalAidNames.ConvertAll(x => new MedicalAidScheme
        {
            Name = x,
            DateAdded = now
        })).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}