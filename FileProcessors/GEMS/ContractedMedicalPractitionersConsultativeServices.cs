using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.GEMS;

public class ContractedMedicalPractitionersConsultativeServices(
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
        new("10", "Anaesthesiologists"),
        new("14", "General Medical Practice"),
        new("15", "Family Physicians"),
        new("16", "Obstetrics and Gynaecology"),
        new("17", "Pulmonology"),
        new("18", "Medicine (Specialist Physician)"),
        new("19", "Gastroenterology"),
        new("20", "Neurology"),
        new("21", "Cardiology"),
        new("24", "Neurosurgery"),
        new("26", "Ophthalmology"),
        new("28", "Orthopaedics"),
        new("30", "Otorhinolaryngology"),
        new("31", "Rheumatology"),
        new("32", "Paediatricians"),
        new("36", "Plastic and Reconstructive Surgery"),
        new("42", "Surgery/Paediatric Surgery"),
        new("44", "Cardiothoracic Surgery"),
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
            case "10":
                return "C";
            case "14":
                return "D";
            case "15":
                return "E";
            case "16":
                return "F";
            case "17":
                return "G";
            case "18":
                return "H";
            case "19":
                return "I";
            case "20":
                return "J";
            case "21":
                return "K";
            case "24":
                return "L";
            case "26":
                return "M";
            case "28":
                return "N";
            case "30":
                return "O";
            case "31":
                return "P";
            case "32":
                return "Q";
            case "36":
                return "R";
            case "42":
                return "S";
            case "44":
                return "T";
            case "46":
                return "U";
            case "114":
                return "V";
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
            await disciplineRepository.InsertAsync(disciplineInternal, false).ConfigureAwait(false);
            return disciplineInternal;
        }

        Discipline discipline = null;
        switch (code)
        {
            case "10":
            case "14":
            case "15":
            case "16":
            case "17":
            case "18":
            case "19":
            case "20":
            case "21":
            case "24":
            case "26":
            case "28":
            case "30":
            case "31":
            case "32":
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