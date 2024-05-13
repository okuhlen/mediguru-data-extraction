using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.GEMS;

public sealed class ContractedDentistsAndDentalSpecialistsFileProcessor(
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
        new("54", "General Dental Practitioner"),
        new("62", "Maxillofacial and Oral Surgery"),
        new("64", "Orthodontics"),
        new("92", "Oral Medicine and Periodontics"),
        new("94", "Prosthodontist"),
        new("98", "Oral Pathology"),
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

            var category = await categoryRepository.FetchByName(parameters.CategoryName).ConfigureAwait(false);
            if (category == null)
            {
                category = new Category
                {
                    Description = parameters.CategoryName,
                    DateAdded = DateTime.Now
                };
                await categoryRepository.InsertAsync(category, false).ConfigureAwait(false);
            }

            var provider = await providerRepository.FetchByName("Government Employees Medical Scheme (GEMS)")
                .ConfigureAwait(false);
            foreach (var (disciplineCode, disciplineName) in _disciplines)
            {
                Console.WriteLine($"Now processing column: {disciplineCode}: {disciplineName}");
                var priceColumn = GetColumnForDisciplineCode(disciplineCode);
                var sheet = document.Worksheets.First();
                var discipline = await GetDiscipline(disciplineCode, disciplineName).ConfigureAwait(false);
                foreach (var row in sheet.Rows())
                {
                    if (row.RowNumber() < parameters.StartingRow)
                    {
                        continue;
                    }
                    if (row.Cell("A").Style.Fill.BackgroundColor.HasValue)
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
                    int tariffCode = default;
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
            case "54":
                return "C";
            case "62":
                return "D";
            case "64":
                return "E";
            case "92":
                return "F";
            case "94":
                return "G";
            case "98":
                return "H";
            default:
                throw new NotSupportedException($"We do not support the provided discipline code: {code}");
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
            await disciplineRepository.InsertAsync(disciplineInternal).ConfigureAwait(false);
            return disciplineInternal;
        }

        Discipline discipline = null;
        switch (code)
        {
            case "54": //general dental practitioner
                discipline = await disciplineRepository.FetchByCode(code).ConfigureAwait(false);
                if (discipline is null)
                {
                    return await InsertDiscipline(discipline).ConfigureAwait(false);
                }

                return discipline;
            case "62": //maxillofacial and oral surgery
                discipline = await disciplineRepository.FetchByCode(code).ConfigureAwait(false);
                if (discipline is null)
                {
                    return await InsertDiscipline(discipline).ConfigureAwait(false);
                }

                return discipline;
            case "64":
                discipline = await disciplineRepository.FetchByCode(code).ConfigureAwait(false);
                if (discipline is null)
                {
                    return await InsertDiscipline(discipline).ConfigureAwait(false);
                }

                return discipline;
            case "92":
                discipline = await disciplineRepository.FetchByCode(code).ConfigureAwait(false);
                if (discipline is null)
                {
                    return await InsertDiscipline(discipline).ConfigureAwait(false);
                }

                return discipline;
            case "94":
                discipline = await disciplineRepository.FetchByCode(code).ConfigureAwait(false);
                if (discipline is null)
                {
                    return await InsertDiscipline(discipline).ConfigureAwait(false);
                }

                return discipline;
            case "98":
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