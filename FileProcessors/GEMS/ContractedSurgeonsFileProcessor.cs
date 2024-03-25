using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.GEMS;

public sealed class ContractedSurgeonsFileProcessor(
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
        new("24", "Neurosurgery"),
        new("26", "Ophthalmology"),
        new("28", "Orthopaedics"),
        new("30", "Otorhinolaryngology"),
        new("36", "Plastic and Reconstructive Surgery"),
        new("42", "Surgery/Paediatric Surgery"),
        new("44", "Cardio Thoracic Surgery"),
        new("46", "Urology"),
        new("114", "Paediatric Surgeon")
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

            var category = await categoryRepository.FetchByName(parameters.CategoryName).ConfigureAwait(false);
            if (category == null)
            {
                category = new Category
                {
                    Description = parameters.CategoryName,
                    DateAdded = DateTime.Now
                };
                await categoryRepository.InsertAsync(category, true).ConfigureAwait(false);
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            var provider = await providerRepository.FetchByName("Government Employees Medical Scheme (GEMS)")
                .ConfigureAwait(false);
            var dataSource = await sourceTypeRepository.FetchByNameAsync("GEMS").ConfigureAwait(false);
            if (dataSource is null)
                throw new Exception($"Missing data source name: GEMS");


            foreach (var (disciplineCode, disciplineName) in _disciplines)
            {
                Console.WriteLine($"Now processing column: {disciplineCode}: {disciplineName}");
                var sheet = document.Worksheets.First();
                var discipline = await GetDiscipline(disciplineCode, disciplineName).ConfigureAwait(false);
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

                    if (row.Cell("A").IsEmpty() || row.Cell("C").IsEmpty())
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
                        IsGovernmentBaselineRate = false,
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
            case "24":
            case "26":
            case "28":
            case "30":
            case "36":
            case "42":
            case "44":
            case "46":
            case "114":
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