using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors;

internal sealed class CategoriesFileProcessor(MediGuruDbContext dbContext, ICategoryRepository categoryRepository)
{
    //categories were taken from: https://www.healthman.co.za/Tariffs/Tariffs2023
    public async Task ProcessAsync()
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            var directory = $"{Directory.GetCurrentDirectory()}/Files/Misc/";
            foreach (var file in Directory.GetFiles(directory, "*.txt"))
            {
                using var reader = new StreamReader(file);
                while (!reader.EndOfStream)
                {
                    var category = new Category 
                    { 
                        Description = reader.ReadLine() ?? throw new NullReferenceException("We need category info here"), 
                        DateAdded = DateTime.Now 
                    };
                    await categoryRepository.InsertAsync(category, false).ConfigureAwait(false);
                }
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
        
    }
}
