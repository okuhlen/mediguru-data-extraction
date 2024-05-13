using ClosedXML.Excel;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors;

public sealed class MedicalAidNamesProcessor(
    IMedicalAidNameRepository medicalAidNameRepository,
    MediGuruDbContext dbContext)
{
    public async Task ProcessAsync()
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

            var medicalAidNames = new List<string>();

            var currentFile = $"{Directory.GetCurrentDirectory()}/Files/Misc/medical_aid_schemes.xlsx";
            var excelDocument = new XLWorkbook(currentFile);
            var excelWorksheet = excelDocument.Worksheets.First() ??
                                 throw new Exception("The excel document does not have any worksheets");

            foreach (var row in excelWorksheet.Rows())
            {
                if (row.RowNumber() == 1)
                    continue; //skip the first header row.
                if (string.IsNullOrEmpty(row.Cell("A").GetText()))
                    throw new Exception("There shouldn't be any rows that have no text");
                medicalAidNames.Add(row.Cell("A").GetText());
            }

            await medicalAidNameRepository.InsertBulk(medicalAidNames).ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
}