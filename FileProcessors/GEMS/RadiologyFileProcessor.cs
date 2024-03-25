using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.GEMS;

public sealed class RadiologyFileProcessor(
    MediGuruDbContext dbContext,
    IProviderProcedureDataSourceTypeRepository sourceTypeRepository,
    ICategoryRepository categoryRepository,
    IProviderRepository providerRepository,
    IDisciplineRepository disciplineRepository,
    IProcedureRepository procedureRepository,
    IProviderProcedureRepository providerProcedureRepository)
{
    private readonly List<(string Code, string Name)> _disciplines = new()
    {
        new("25", "Nuclear Medicine"),
        new("38", "Radiology")
    };

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

            var provider = await providerRepository.FetchByName("Government Employees Medical Scheme (GEMS)")
                .ConfigureAwait(false);
            var category = await categoryRepository.FetchByName(parameters.CategoryName).ConfigureAwait(false);
            if (category is null)
            {
                category = new Category
                {
                    DateAdded = DateTime.Now,
                    Description = parameters.CategoryName,
                };
                await categoryRepository.InsertAsync(category, false).ConfigureAwait(false);
            }

            foreach (var (disciplineCode, disciplineName) in _disciplines)
            {
                Console.WriteLine($"Now processing column: {disciplineCode}: {disciplineName}");
                var priceColumn = GetColumnForDisciplineCode(disciplineCode);
                var sheet = document.Worksheets.First();
                var discipline = await disciplineRepository.FetchByCode(disciplineCode).ConfigureAwait(false);
                if (discipline is null)
                {
                    discipline = new Discipline
                    {
                        Code = disciplineCode,
                        DateAdded = DateTime.Now,
                        Description = disciplineName,
                        SubCode = "0"
                    };
                    await disciplineRepository.InsertAsync(discipline, false).ConfigureAwait(false);
                }

                foreach (var row in sheet.Rows())
                {
                    if (row.RowNumber() < parameters.StartingRow)
                    {
                        continue;
                    }

                    if (parameters.EndingRow.HasValue && row.RowNumber() >= parameters.EndingRow)
                    {
                        Console.WriteLine($"End of column: {disciplineCode}: {disciplineName}");
                        break;
                    }

                    if (row.Cell("A").IsEmpty() || row.Cell(priceColumn).IsEmpty())
                        continue;

                    var tariffCodeText = row.Cell("A").GetString().Trim();
                    if (string.IsNullOrEmpty(tariffCodeText) || string.IsNullOrWhiteSpace(tariffCodeText))
                    {
                        continue;
                    }

                    var procedure = await procedureRepository
                        .FetchByCodeAndCategoryId(tariffCodeText, category.CategoryId)
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

                    var tariffPrice = FormattingHelpers.FormatProcedurePrice(row.Cell(priceColumn).GetString());
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
                        IsGovernmentBaselineRate = false,
                        AdditionalNotes = parameters.AdditionalNotes,
                        IsContracted = false,
                    };
                    await providerProcedureRepository.InsertAsync(providerProcedure, false).ConfigureAwait(false);
                }
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
            Console.WriteLine($"DONE PROCESSING FILE: {parameters.FileLocation}");
        }).ConfigureAwait(false);
    }

    private string GetColumnForDisciplineCode(string code)
    {
        switch (code)
        {
            case "25":
                return "C";
            case "38":
                return "D";
            default:
                throw new NotSupportedException($"We do not support the provided discipline code: {code}");
        }
    }
    
    
}