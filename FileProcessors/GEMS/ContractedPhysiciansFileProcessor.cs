using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.GEMS;

public sealed class ContractedPhysiciansFileProcessor
{
    private readonly List<(string DisciplineCode, String DisciplineName)> _disciplines;
    private readonly MediGuruDbContext _dbContext;
    private readonly IProviderProcedureDataSourceTypeRepository _sourceTypeRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IDisciplineRepository _disciplineRepository;
    private readonly IProcedureRepository _procedureRepository;
    private readonly IProviderProcedureRepository _providerProcedureRepository;

    public ContractedPhysiciansFileProcessor(MediGuruDbContext dbContext,
        IProviderProcedureDataSourceTypeRepository sourceTypeRepository,
        ICategoryRepository categoryRepository, IProviderRepository providerRepository,
        IDisciplineRepository disciplineRepository, IProcedureRepository procedureRepository,
        IProviderProcedureRepository providerProcedureRepository)
    {
        _dbContext = dbContext;
        _sourceTypeRepository = sourceTypeRepository;
        _categoryRepository = categoryRepository;
        _providerRepository = providerRepository;
        _disciplineRepository = disciplineRepository;
        _procedureRepository = procedureRepository;
        _providerProcedureRepository = providerProcedureRepository;
        _disciplines = new()
        {
            new("17", "Pulmonology"),
            new("18", "Medicine (Specialist Physician)"),
            new("19", "Gastroenterology"),
            new("20", "Neurology"),
            new("21", "Cardiology"),
            new("31", "Rheumatology"),
        };
    }

    public async Task ProcessAsync(ProcessFileParameters parameters)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            Console.WriteLine($"Now Processing File: {parameters.FileLocation}");
            using var document = new XLWorkbook(parameters.FileLocation);
            if (string.IsNullOrEmpty(parameters.FileLocation))
            {
                throw new Exception("File not present");
            }
            var dataSource = await _sourceTypeRepository.FetchByNameAsync("GEMS").ConfigureAwait(false);
            if (dataSource is null)
                throw new Exception($"Missing data source name: GEMS");

            var provider = await _providerRepository.FetchByName("Government Employees Medical Scheme (GEMS)")
                .ConfigureAwait(false);

            var proceduresList = await _procedureRepository.FetchAll().ConfigureAwait(false);
            using var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            foreach (var (disciplineCode, disciplineName) in _disciplines)
            {
                var category = await GetCategory(disciplineCode).ConfigureAwait(false);
                Console.WriteLine($"Now processing column: {disciplineCode}: {disciplineName}");
                var sheet = document.Worksheets.First();
                var discipline = await _disciplineRepository.FetchByCode(disciplineCode).ConfigureAwait(false);
                if (discipline is null)
                {
                    discipline = new Discipline
                    {
                        Code = disciplineCode,
                        Description = disciplineName,
                        DateAdded = DateTime.Now,
                        SubCode = "0"
                    };
                    await _disciplineRepository.InsertAsync(discipline, shouldSaveNow: false).ConfigureAwait(false);
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

                    if (row.Cell("A").IsEmpty() || row.Cell("C").IsEmpty())
                        continue;

                    var tariffCodeText = row.Cell("A").GetString().Trim();
                    if (string.IsNullOrEmpty(tariffCodeText) || string.IsNullOrWhiteSpace(tariffCodeText))
                    {
                        continue;
                    }

                    var procedure =
                        proceduresList.FirstOrDefault(x =>
                            string.Equals(x.Code, tariffCodeText, StringComparison.OrdinalIgnoreCase) &&
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
                        await _procedureRepository.InsertAsync(procedure, false).ConfigureAwait(false);
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
                        IsGovernmentBaselineRate = false,
                        IsNonContracted = parameters.IsNonContracted == true,
                        AdditionalNotes = parameters.AdditionalNotes,
                        IsContracted = parameters.IsContracted == true,
                    };
                    await _providerProcedureRepository.InsertAsync(providerProcedure, false).ConfigureAwait(false);
                }
            }

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
            Console.WriteLine($"DONE PROCESSING FILE: {parameters.FileLocation}");
        }).ConfigureAwait(false);
    }

    private async Task<Category> GetCategory(string code)
    {
        switch (code)
        {
            case "17":
                var pulmonologyCat = await _categoryRepository.FetchByName("Pulmonology").ConfigureAwait(false);
                if (pulmonologyCat is null)
                {
                    pulmonologyCat = new Category
                    {
                        DateAdded = DateTime.Now,
                        Description = "Pulmonology",
                    };
                    await _categoryRepository.InsertAsync(pulmonologyCat).ConfigureAwait(false);
                }

                return pulmonologyCat;
            case "18":
                var medicineSpecialist = await _categoryRepository.FetchByName("Medicine (Specialist Physician)")
                    .ConfigureAwait(false);
                if (medicineSpecialist is null)
                {
                    medicineSpecialist = new Category
                    {
                        DateAdded = DateTime.Now,
                        Description = "Medicine (Specialist Physician)",
                    };
                    await _categoryRepository.InsertAsync(medicineSpecialist).ConfigureAwait(false);
                }

                return medicineSpecialist;
            case "19":
                var specialistCat = await _categoryRepository.FetchByName("Gastroenterology").ConfigureAwait(false);
                if (specialistCat is null)
                {
                    specialistCat = new Category
                    {
                        Description = "Gastroenterology",
                        DateAdded = DateTime.Now
                    };
                    await _categoryRepository.InsertAsync(specialistCat).ConfigureAwait(false);
                }

                return specialistCat;
            case "20":
                var neurologyCat = await _categoryRepository.FetchByName("Neurology").ConfigureAwait(false);
                if (neurologyCat is null)
                {
                    neurologyCat = new Category
                    {
                        Description = "Neurology",
                        DateAdded = DateTime.Now
                    };

                    await _categoryRepository.InsertAsync(neurologyCat).ConfigureAwait(false);
                }

                return neurologyCat;
            case "21":
                var cardiologyCat = await _categoryRepository.FetchByName("Cardiology").ConfigureAwait(false);
                if (cardiologyCat is null)
                {
                    cardiologyCat = new Category
                    {
                        Description = "Cardiology",
                        DateAdded = DateTime.Now
                    };
                    await _categoryRepository.InsertAsync(cardiologyCat).ConfigureAwait(false);
                }

                return cardiologyCat;
            case "31":
                var rheumaCat = await _categoryRepository.FetchByName("Rheumatology").ConfigureAwait(false);
                if (rheumaCat is null)
                {
                    rheumaCat = new Category
                    {
                        Description = "Rheumatology",
                        DateAdded = DateTime.Now
                    };
                    await _categoryRepository.InsertAsync(rheumaCat).ConfigureAwait(false);
                }

                return rheumaCat;
            default:
                throw new NotSupportedException($"The code provided is not supported: {code}");
        }
    }
}