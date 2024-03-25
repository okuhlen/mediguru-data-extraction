using System.Linq.Expressions;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly MediGuruDbContext _dbContext;

    public CategoryRepository(MediGuruDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Category> FetchByName(string name)
    {
        return await _dbContext.Categories.FirstOrDefaultAsync(x => x.Description == name).ConfigureAwait(false);
    }

    public async Task<bool> Exists(string categoryName)
    {
        return await _dbContext.Categories.AnyAsync(x => x.Description == categoryName);
    }

    public async Task<List<Category>> FetchAll()
    {
        return await _dbContext.Categories.ToListAsync().ConfigureAwait(false);
    }

    public async Task<Category> FetchById(string id)
    {
        return await _dbContext.Categories.FirstAsync(x => x.CategoryId == id).ConfigureAwait(false);
    }

    public async Task<int> Count()
    {
        return await _dbContext.Categories.CountAsync().ConfigureAwait(false);
    }

    public async Task InsertAsync(Category category, bool shouldSaveNow = true)
    {
        var dbCategory = new Category
        {
            Description = category.Description,
            DateAdded = DateTime.Now,
        };

        var addedEntity = await _dbContext.Categories.AddAsync(dbCategory).ConfigureAwait(false);
        if (shouldSaveNow)
        {
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        
        category.CategoryId = addedEntity.Entity.CategoryId;
    }
}
