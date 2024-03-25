using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors;

public sealed class WoolTruSurgeonsFileProcessor(
    IProcedureRepository procedureRepository,
    IProviderRepository providerRepository,
    MediGuruDbContext dbContext,
    IProviderProcedureTypeRepository providerProcedureTypeRepository,
    IProviderProcedureRepository providerProcedureRepository,
    IProviderProcedureDataSourceTypeRepository dataSourceRepository,
    IDisciplineRepository disciplineRepository,
    ICategoryRepository categoryRepository)
{
    private readonly IProviderProcedureTypeRepository _providerProcedureTypeRepository = providerProcedureTypeRepository;

    private List<(string DisciplineCode, String DisciplineName)> _disciplines = new List<(string Code, string Name)>
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

    //we use the surgeons discipline codes as defined on the GEMS source files here. 
    public async Task ProcessAsync()
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            var provider = await providerRepository.FetchByName("WoolTru Healthcare Fund").ConfigureAwait(false);
            var dataSourceType = await dataSourceRepository.FetchByNameAsync("WoolTru").ConfigureAwait(false);
            var category = await categoryRepository.FetchByName("Surgeons").ConfigureAwait(false);
            var disciplines = await disciplineRepository.FetchAll().ConfigureAwait(false);

            Console.WriteLine("Now processing WoolTru surgeons data file. This will take a while. Please wait...");
            foreach (var (disciplineCode, disciplineName) in _disciplines)
            {
                using var streamReader = new StreamReader($"{Environment.CurrentDirectory}/Files/WoolTru/surgeons.txt");
                var discipline = disciplines.First(x => x.Code == disciplineCode);
                int lineNumber = 0;
                ProviderProcedure? providerProcedure = null;
                while (!streamReader.EndOfStream)
                {
                    //read the procedure code
                    if (lineNumber == 0)
                    {
                        var code = await streamReader.ReadLineAsync() ??
                                   throw new NullReferenceException("No tariff code was read");
                        if (code.StartsWith("0"))
                            code = code.Substring(1, code.Length - 1);

                        var procedure = await procedureRepository.FetchByCodeAndCategoryId(code, category.CategoryId);
                        if (procedure == null)
                        {
                            continue;
                        }

                        providerProcedure = new ProviderProcedure
                        {
                            ProcedureId = procedure.ProcedureId,
                            ProviderProcedureDataSourceTypeId = dataSourceType.ProviderProcedureDataSourceTypeId,
                            ProviderId = provider.ProviderId,
                            DisciplineId = discipline.DisciplineId,
                            YearValidFor = 2023,
                        };
                        lineNumber++;
                        continue;
                    }

                    if (lineNumber == 1)
                    {
                        await streamReader.ReadLineAsync();
                        lineNumber++;
                        continue;
                    }

                    if (lineNumber == 2)
                    {
                        var price = streamReader.ReadLine().Trim();
                        ArgumentNullException.ThrowIfNull(price);
                        var formattedPrice = GetFormattedPrice(price);
                        providerProcedure.Price = formattedPrice;
                        providerProcedure.AdditionalNotes = null;
                        await providerProcedureRepository.InsertAsync(providerProcedure, false).ConfigureAwait(false);
                        providerProcedure = null;
                        lineNumber = 0;
                    }
                }
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private static double? GetFormattedPrice(string field)
    {
        if (field.StartsWith("R"))
            field = field.Substring(1, field.Length - 1);
        if (field.Contains(" "))
            field = field.Replace(" ", "");

        if (double.TryParse(field, out var formattedPrice))
            return formattedPrice;
        return null;
    }
}