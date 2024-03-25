using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class SearchDataRepository(MediGuruDbContext dbContext) : ISearchDataRepository
{
    public async Task<bool> IsEmpty()
    {
        return !await dbContext.SearchDatas.AnyAsync().ConfigureAwait(false);
    }
}