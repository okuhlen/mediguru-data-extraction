using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class SearchDataPointRepository : ISearchDataPointRepository
{
    private readonly MediGuruDbContext _dbContext;

    public SearchDataPointRepository(MediGuruDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Insert(SearchDataPoint searchDataPoint)
    {
        var dbDataPoint = new SearchDataPoint
        {
            DateAdded = DateTime.Now,
            CategoryId = searchDataPoint.CategoryId,
            DisciplineId = searchDataPoint.DisciplineId,
            IsOfficialSource = searchDataPoint.IsOfficialSource,
            IsUserSupplied = searchDataPoint.IsUserSupplied,
            IsThirdPartySource = searchDataPoint.IsThirdPartySource,
            MedicalAidSchemeId = searchDataPoint.MedicalAidSchemeId,
            SearchDataId = searchDataPoint.SearchDataId,
            MedicalAidPlanName = searchDataPoint.MedicalAidPlanName,
            Price = searchDataPoint.Price,
            YearValidFor = searchDataPoint.YearValidFor,
        };

        var entity = await _dbContext.SearchDataPoints.AddAsync(dbDataPoint).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        searchDataPoint.Id = entity.Entity.Id;
    }

    public async Task<IList<SearchDataPoint>> FetchBySearchDataId(string searchDataId)
    {
        return await _dbContext.SearchDataPoints.Where(x => x.SearchDataId == searchDataId).ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> IsEmpty()
    {
        return !await _dbContext.SearchDataPoints.AnyAsync().ConfigureAwait(false);
    }
}