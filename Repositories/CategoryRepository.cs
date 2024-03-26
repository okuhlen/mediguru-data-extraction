using MediGuru.DataExtractionTool.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class CategoryRepository(MediGuruDbContext dbContext) : ICategoryRepository
{
    public async Task<Category> FetchByName(string name)
    {
        return await dbContext.Categories.FirstOrDefaultAsync(x => x.Description == name).ConfigureAwait(false);
    }

    public async Task<List<Category>> FetchAll()
    {
        return await dbContext.Categories.ToListAsync().ConfigureAwait(false);
    }

    public async Task InsertAsync(Category category, bool shouldSaveNow = true)
    {
        var dbCategory = new Category
        {
            Description = category.Description,
            DateAdded = DateTime.Now,
        };

        var addedEntity = await dbContext.Categories.AddAsync(dbCategory).ConfigureAwait(false);
        if (shouldSaveNow)
        {
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        
        category.CategoryId = addedEntity.Entity.CategoryId;
    }
}
