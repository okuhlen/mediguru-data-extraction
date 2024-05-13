using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors;

//disciplines taken from: https://www.bisolutions.co.za/reports/disciplines.php
internal class DisciplinesFileProcessor(IDisciplineRepository disciplineRepository, MediGuruDbContext dbContext)
{
    public async Task ProcessAsync()
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

            var fileDirectory = $"{Directory.GetCurrentDirectory()}/Files/Disciplines";
            foreach (var file in Directory.GetFiles(fileDirectory, "*.xlsx"))
            {
                using var document = new XLWorkbook(file);
                var sheet = document.Worksheets.First();

                foreach (var row in sheet.Rows())
                {
                    if (row.RowNumber() == 1)
                        continue; //skip the first row which contains headers

                    var discipline = new Discipline
                    {
                        Code = Convert.ToString(row.Cell("A").Value.GetNumber()),
                        SubCode = Convert.ToString(row.Cell("B").Value.GetNumber()),
                        Description = row.Cell("C").GetText().Trim(),
                        DateAdded = DateTime.Now
                    };

                    await disciplineRepository.InsertAsync(discipline, false).ConfigureAwait(false);
                }
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
        
    }
}
