using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.GEMS;

public class ContractedOralHygienistFileProcessor(
    MediGuruDbContext dbContext,
    IProviderProcedureDataSourceTypeRepository sourceTypeRepository,
    ICategoryRepository categoryRepository,
    IProviderRepository providerRepository,
    IDisciplineRepository disciplineRepository,
    IProcedureRepository procedureRepository,
    IProviderProcedureRepository providerProcedureRepository)
{
    private const string DisciplineCode = "113";

    public async Task ProcessAsync(ProcessFileParameters parameters)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            Console.WriteLine($"Now Processing File: {parameters.FileLocation}");
            using var document = new XLWorkbook(parameters.FileLocation);

            if (string.IsNullOrEmpty(parameters.FileLocation))
            {
                throw new Exception("File not present");
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            var dataSource = await sourceTypeRepository.FetchByNameAsync("GEMS").ConfigureAwait(false);
            if (dataSource is null)
                throw new Exception($"Missing data source name: GEMS");

            var category = await categoryRepository.FetchByName(parameters.CategoryName).ConfigureAwait(false);
            var provider = await providerRepository.FetchByName("Government Employees Medical Scheme (GEMS)")
                .ConfigureAwait(false);
            var discipline = await disciplineRepository.FetchByCode(DisciplineCode).ConfigureAwait(false);
            if (discipline is null)
            {
                discipline = new Discipline
                {
                    Description = "Oral Hygienists",
                    Code = DisciplineCode,
                    DateAdded = DateTime.Now,
                    SubCode = "0",
                };
                await disciplineRepository.InsertAsync(discipline, false).ConfigureAwait(false);
            }

            if (category == null)
            {
                category = new Category
                {
                    Description = parameters.CategoryName,
                    DateAdded = DateTime.Now
                };
                await categoryRepository.InsertAsync(category, false).ConfigureAwait(false);
            }

            var sheet = document.Worksheets.First();
            foreach (var row in sheet.Rows())
            {
                if (!parameters.RowsToSkip.IsNullOrEmpty() && parameters.RowsToSkip.Contains(row.RowNumber()))
                {
                    Console.WriteLine($"Row {row.RowNumber()} skipped owing to specifications");
                    continue;
                }
                
                if (row.RowNumber() < parameters.StartingRow)
                {
                    continue;
                }
                if (row.Cell("A").Style.Fill.BackgroundColor.HasValue)
                {
                    continue;
                }

                if (row.Cell("A").IsEmpty() || row.Cell("C").IsEmpty())
                    continue;

                var tariffCodeText = row.Cell("A").GetString().Trim();
                if (string.IsNullOrEmpty(tariffCodeText) || string.IsNullOrWhiteSpace(tariffCodeText))
                {
                    continue;
                }
                
                if (!int.TryParse(tariffCodeText, out _))
                {
                    Console.WriteLine($"Could not convert {tariffCodeText}. On file {parameters.FileLocation} in row: {row.RowNumber()}");
                    continue;
                }

                var procedure = await procedureRepository.FetchByCodeAndCategoryId(tariffCodeText, category.CategoryId)
                    .ConfigureAwait(false);
                if (procedure is null)
                {
                    procedure = new Procedure
                    {
                        Code = tariffCodeText,
                        CategoryId = category.CategoryId,
                        CodeDescriptor = row.Cell("B").GetString().Trim(),
                        CreatedDate = DateTime.Now,
                    };
                    await procedureRepository.InsertAsync(procedure, false).ConfigureAwait(false);
                }

                var tariffPrice = FormattingHelpers.FormatProcedurePrice(row.Cell("C").GetString());
                var providerProcedure = new ProviderProcedure
                {
                    Price = tariffPrice,
                    DisciplineId = discipline.DisciplineId,
                    Discipline = discipline,
                    ProcedureId = procedure.ProcedureId,
                    ProviderProcedureDataSourceTypeId = dataSource.ProviderProcedureDataSourceTypeId,
                    ProviderId = provider.ProviderId,
                    Provider = provider,
                    YearValidFor = parameters.YearValidFor,
                    DateAdded = DateTime.Now,
                    IsContracted = parameters.IsContracted == true,
                    IsNonContracted = parameters.IsNonContracted == true,
                    AdditionalNotes = parameters.AdditionalNotes,
                };
                await providerProcedureRepository.InsertAsync(providerProcedure, false).ConfigureAwait(false);
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
            Console.WriteLine($"DONE PROCESSING FILE: {parameters.FileLocation}");
        }).ConfigureAwait(false);
    }
}