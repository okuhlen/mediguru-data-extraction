using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.GEMS;

public sealed class ContractedMedicalPractitionersFileProcessor(
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
        new("14", "General Medical Practice"),
        new("16", "Obstetrics and Gynaecology"),
        new("32", "Paediatricians"),
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
            foreach (var (disciplineCode, disciplineName) in _disciplines)
            {
                var category = await GetCategory(disciplineCode).ConfigureAwait(false);
                Console.WriteLine($"Now processing column: {disciplineCode}: {disciplineName}");
                var priceColumn = GetColumnForDisciplineCode(disciplineCode);
                var sheet = document.Worksheets.First();
                var discipline = await GetDiscipline(disciplineCode, disciplineName).ConfigureAwait(false);
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
                    if (!int.TryParse(tariffCodeText, out _))
                    {
                        Console.WriteLine($"Could not convert {tariffCodeText}. On file {parameters.FileLocation} in row: {row.RowNumber()}");
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
                        AdditionalNotes = parameters.AdditionalNotes,
                        IsContracted = parameters.IsContracted == true,
                        IsNonContracted = parameters.IsNonContracted == true,
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
            case "14":
                return "C";
            case "16":
                return "E";
            case "32":
                return "G";
            default:
                throw new NotSupportedException($"We do not support the provided discipline code: {code}");
        }
    }

    private async Task<Category> GetCategory(string code)
    {
        switch (code)
        {
            case "14":
            case "16":
                var category = await categoryRepository.FetchByName("Uncategorized").ConfigureAwait(false);
                return category;
            case "32":
                return await categoryRepository.FetchByName("Paediatrician").ConfigureAwait(false);
            default:
                throw new NotSupportedException($"The code provided is not supported: {code}");
        }
    }

    private async Task<Discipline> GetDiscipline(string code, string disciplineName)
    {
        async Task<Discipline> InsertDiscipline(Discipline disciplineInternal)
        {
            disciplineInternal = new Discipline
            {
                Code = code,
                SubCode = "0",
                DateAdded = DateTime.Now,
                Description = disciplineName,
            };
            await disciplineRepository.InsertAsync(disciplineInternal, false).ConfigureAwait(false);
            return disciplineInternal;
        }

        Discipline discipline = null;
        switch (code)
        {
            case "14":
            case "16":
            case "32":
                discipline = await disciplineRepository.FetchByCode(code).ConfigureAwait(false);
                if (discipline is null)
                {
                    return await InsertDiscipline(discipline).ConfigureAwait(false);
                }

                return discipline;
            default:
                throw new NotSupportedException($"The entered discipline code is not supported: {code}");
        }
    }
}