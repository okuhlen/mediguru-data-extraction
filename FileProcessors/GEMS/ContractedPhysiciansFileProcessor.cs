using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.GEMS;

public sealed class ContractedPhysiciansFileProcessor(
    MediGuruDbContext dbContext,
    IProviderProcedureDataSourceTypeRepository sourceTypeRepository,
    ICategoryRepository categoryRepository,
    IProviderRepository providerRepository,
    IDisciplineRepository disciplineRepository,
    IProcedureRepository procedureRepository,
    IProviderProcedureRepository providerProcedureRepository)
{
    private readonly List<(string DisciplineCode, String DisciplineName)> _disciplines = new()
    {
        new("17", "Pulmonology"),
        new("18", "Medicine (Specialist Physician)"),
        new("19", "Gastroenterology"),
        new("20", "Neurology"),
        new("21", "Cardiology"),
        new("31", "Rheumatology"),
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
            var dataSource = await sourceTypeRepository.FetchByNameAsync("GEMS").ConfigureAwait(false);
            if (dataSource is null)
                throw new Exception($"Missing data source name: GEMS");

            var provider = await providerRepository.FetchByName("Government Employees Medical Scheme (GEMS)")
                .ConfigureAwait(false);

            var proceduresList = await procedureRepository.FetchAll().ConfigureAwait(false);
            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            foreach (var (disciplineCode, disciplineName) in _disciplines)
            {
                var category = await GetCategory(disciplineCode).ConfigureAwait(false);
                Console.WriteLine($"Now processing column: {disciplineCode}: {disciplineName}");
                var sheet = document.Worksheets.First();
                var discipline = await disciplineRepository.FetchByCode(disciplineCode).ConfigureAwait(false);
                if (discipline is null)
                {
                    discipline = new Discipline
                    {
                        Code = disciplineCode,
                        Description = disciplineName,
                        DateAdded = DateTime.Now,
                        SubCode = "0"
                    };
                    await disciplineRepository.InsertAsync(discipline, shouldSaveNow: false).ConfigureAwait(false);
                }

                foreach (var row in sheet.Rows())
                {
                    if (!parameters.RowsToSkip.IsNullOrEmpty() && parameters.RowsToSkip.Contains(row.RowNumber()))
                    {
                        Console.WriteLine($"Row {row.RowNumber()} is skipped owing to specifications");
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

                    var procedure =
                        proceduresList.FirstOrDefault(x =>
                            x.Code == tariffCodeText &&
                            string.Equals(x.CategoryId, category.CategoryId, StringComparison.OrdinalIgnoreCase));
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
                        proceduresList.Add(procedure);
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
                        IsNonContracted = parameters.IsNonContracted == true,
                        AdditionalNotes = parameters.AdditionalNotes,
                        IsContracted = parameters.IsContracted == true,
                    };
                    await providerProcedureRepository.InsertAsync(providerProcedure, false).ConfigureAwait(false);
                }
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
            Console.WriteLine($"DONE PROCESSING FILE: {parameters.FileLocation}");
        }).ConfigureAwait(false);
    }

    private async Task<Category> GetCategory(string code)
    {
        switch (code)
        {
            case "17":
                var pulmonologyCat = await categoryRepository.FetchByName("Pulmonology").ConfigureAwait(false);
                if (pulmonologyCat is null)
                {
                    pulmonologyCat = new Category
                    {
                        DateAdded = DateTime.Now,
                        Description = "Pulmonology",
                    };
                    await categoryRepository.InsertAsync(pulmonologyCat).ConfigureAwait(false);
                }

                return pulmonologyCat;
            case "18":
                var medicineSpecialist = await categoryRepository.FetchByName("Medicine (Specialist Physician)")
                    .ConfigureAwait(false);
                if (medicineSpecialist is null)
                {
                    medicineSpecialist = new Category
                    {
                        DateAdded = DateTime.Now,
                        Description = "Medicine (Specialist Physician)",
                    };
                    await categoryRepository.InsertAsync(medicineSpecialist).ConfigureAwait(false);
                }

                return medicineSpecialist;
            case "19":
                var specialistCat = await categoryRepository.FetchByName("Gastroenterology").ConfigureAwait(false);
                if (specialistCat is null)
                {
                    specialistCat = new Category
                    {
                        Description = "Gastroenterology",
                        DateAdded = DateTime.Now
                    };
                    await categoryRepository.InsertAsync(specialistCat).ConfigureAwait(false);
                }

                return specialistCat;
            case "20":
                var neurologyCat = await categoryRepository.FetchByName("Neurology").ConfigureAwait(false);
                if (neurologyCat is null)
                {
                    neurologyCat = new Category
                    {
                        Description = "Neurology",
                        DateAdded = DateTime.Now
                    };

                    await categoryRepository.InsertAsync(neurologyCat).ConfigureAwait(false);
                }

                return neurologyCat;
            case "21":
                var cardiologyCat = await categoryRepository.FetchByName("Cardiology").ConfigureAwait(false);
                if (cardiologyCat is null)
                {
                    cardiologyCat = new Category
                    {
                        Description = "Cardiology",
                        DateAdded = DateTime.Now
                    };
                    await categoryRepository.InsertAsync(cardiologyCat).ConfigureAwait(false);
                }

                return cardiologyCat;
            case "31":
                var rheumaCat = await categoryRepository.FetchByName("Rheumatology").ConfigureAwait(false);
                if (rheumaCat is null)
                {
                    rheumaCat = new Category
                    {
                        Description = "Rheumatology",
                        DateAdded = DateTime.Now
                    };
                    await categoryRepository.InsertAsync(rheumaCat).ConfigureAwait(false);
                }

                return rheumaCat;
            default:
                throw new NotSupportedException($"The code provided is not supported: {code}");
        }
    }
}