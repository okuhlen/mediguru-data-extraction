using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class SearchDataPointRepository(MediGuruDbContext dbContext) : ISearchDataPointRepository
{
    public async Task<bool> IsEmpty()
    {
        return !await dbContext.SearchDataPoints.AnyAsync().ConfigureAwait(false);
    }
}