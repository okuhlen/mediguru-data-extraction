using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.Repositories
{
    public interface ICategoryRepository
    {
        Task InsertAsync(Category category, bool shouldSaveNow = true);

        Task<Category> FetchByName(string name);

        Task<bool> Exists(string categoryName);

        Task<List<Category>> FetchAll();

        Task<Category> FetchById(string id);
    }
}
