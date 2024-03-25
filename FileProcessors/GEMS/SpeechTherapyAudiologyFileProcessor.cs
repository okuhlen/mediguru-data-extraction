using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.GEMS;

public sealed class SpeechTherapyAudiologyFileProcessor(
    MediGuruDbContext dbContext,
    IProviderProcedureDataSourceTypeRepository sourceTypeRepository,
    ICategoryRepository categoryRepository,
    IProviderRepository providerRepository,
    IDisciplineRepository disciplineRepository,
    IProcedureRepository procedureRepository,
    IProviderProcedureRepository providerProcedureRepository)
{
    public async Task ProcessAsync(ProcessFileParameters parameters, IList<(string Code, string Name, string SubCode)> disciplines)
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
            if (category is null)
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
            foreach (var (disciplineCode, disciplineName, subCode) in disciplines)
            {
                Console.WriteLine($"Now processing column: {disciplineCode}: {disciplineName}");
                var priceColumn = GetColumnForDisciplineCode(disciplineCode, subCode, parameters.YearValidFor);
                var sheet = document.Worksheets.First();
                var discipline = await disciplineRepository.FetchByCodeAndSubCode(disciplineCode, subCode)
                    .ConfigureAwait(false);
                if (discipline is null)
                {
                    discipline = new Discipline
                    {
                        Code = disciplineCode,
                        SubCode = subCode,
                        DateAdded = DateTime.Now,
                        Description = disciplineName
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
                        IsContracted = true,
                    };
                    await providerProcedureRepository.InsertAsync(providerProcedure, false).ConfigureAwait(false);
                }
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
            Console.WriteLine($"DONE PROCESSING FILE: {parameters.FileLocation}");
        }).ConfigureAwait(false);
    }

    private string GetColumnForDisciplineCode(string code, string subCode, int yearValidFor)
    {
        switch (yearValidFor)
        {
            case 2024:
                if (code.Equals("82", StringComparison.InvariantCultureIgnoreCase) &&
                    subCode.Equals("1", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "C";
                }

                if (code.Equals("82", StringComparison.InvariantCultureIgnoreCase) &&
                    subCode.Equals("2", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "D";
                }

                throw new NotSupportedException(
                    $"The code and sub-code are not supported: [code: {code}; sub-code: {subCode}]");
            case 2023:
                if (code.Equals("82", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "C";
                }

                if (code.Equals("83", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "D";
                }
                throw new NotSupportedException(
                    $"The code and sub-code are not supported: [code: {code}; sub-code: {subCode}]");
            default:
                throw new NotSupportedException($"The provided year is not supported: {yearValidFor}");
        }
        
    }
}