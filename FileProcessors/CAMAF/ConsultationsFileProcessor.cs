using ClosedXML.Excel;
using MediGuru.DataExtractionTool.Constants;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.CAMAF;

public sealed class ConsultationsFileProcessor(MediGuruDbContext dbContext,
    IProviderProcedureDataSourceTypeRepository sourceTypeRepository,
    ICategoryRepository categoryRepository,
    IProviderRepository providerRepository,
    IDisciplineRepository disciplineRepository,
    IProcedureRepository procedureRepository,
    IProviderProcedureRepository providerProcedureRepository)
{
    private readonly List<string> _tariffCodes = new()
    {
        "0190",
        "0191",
        "0192",
    };
    public async Task ProcessAsync()
    {
        var executionStrategy = dbContext.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            using var document = new XLWorkbook(CAMAFFileConstants.GeneralPractitionersAndSpecialistsFile);
            var category = await categoryRepository.FetchByName("Uncategorized").ConfigureAwait(false);
            var provider = await providerRepository.FetchByName("Chartered Accountants (SA) Medical Aid Fund (CAMAF)").ConfigureAwait(false);

            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            foreach (var row in document.Worksheets.First().Rows())
            {
                if (row.RowNumber() <= 4)
                {
                    continue;
                }

                var disciplineCode = row.Cell("A").GetString();
                var discipline = await disciplineRepository.FetchByCode(disciplineCode).ConfigureAwait(false);
                if (discipline is null)
                {
                    discipline = new Discipline
                    {
                        DateAdded = DateTime.Now,
                        Description = row.Cell("B").GetString().Trim(),
                        SubCode = "0",
                        Code = disciplineCode,
                    };
                    await disciplineRepository.InsertAsync(discipline, false).ConfigureAwait(false);
                }
                
                //Insert Column C pricing - CAMAF base tariff.
                foreach (var tariffCode in _tariffCodes)
                {
                    var procedure = await procedureRepository.FetchByCodeAndCategoryId(tariffCode, "Uncategorized")
                        .ConfigureAwait(false);
                    if (procedure is null)
                    {
                        procedure = new Procedure
                        {
                            CreatedDate = DateTime.Now,
                            CategoryId = category.CategoryId,
                            CodeDescriptor = "Consultation",
                            Code = tariffCode,
                        };
                        await procedureRepository.InsertAsync(procedure, false).ConfigureAwait(false);
                    }

                    ArgumentNullException.ThrowIfNull(discipline, nameof(discipline));
                    ArgumentNullException.ThrowIfNull(procedure, nameof(procedure));
                    ArgumentNullException.ThrowIfNull(provider, nameof(provider));
                    
                    var baseProviderProcedure = new ProviderProcedure
                    {
                        DateAdded = DateTime.Now,
                        YearValidFor = 2024,
                        IsContracted = false,
                        AdditionalNotes = "CAMAF Base Tariff",
                        Price = CAMAFPriceHelper.GetPrice(row.Cell("C").GetString().Trim()),
                        ProviderId = provider.ProviderId,
                        DisciplineId = discipline.DisciplineId,
                        ProcedureId = procedure.ProcedureId,
                    };

                    var percent80ProviderProcedure = new ProviderProcedure
                    {
                        DateAdded = DateTime.Now,
                        YearValidFor = 2024,
                        IsContracted = false,
                        AdditionalNotes = "CAMAF 80% base tariff",
                        Price = CAMAFPriceHelper.GetPrice(row.Cell("D").GetString().Trim()),
                        ProviderId = provider.ProviderId,
                        DisciplineId = discipline.DisciplineId,
                        ProcedureId = procedure.ProcedureId,
                    };

                    var percent100ProviderProcedure = new ProviderProcedure
                    {
                        DateAdded = DateTime.Now,
                        YearValidFor = 2024,
                        IsContracted = false,
                        AdditionalNotes = "100% CAMAF base tariff",
                        Price = CAMAFPriceHelper.GetPrice(row.Cell("E").GetString().Trim()),
                        ProcedureId = procedure.ProcedureId,
                        ProviderId = provider.ProviderId,
                        DisciplineId = discipline.DisciplineId,
                    };

                    await providerProcedureRepository.InsertAsync(percent100ProviderProcedure, false)
                        .ConfigureAwait(false);
                    await providerProcedureRepository.InsertAsync(percent80ProviderProcedure, false)
                        .ConfigureAwait(false);
                    await providerProcedureRepository.InsertAsync(baseProviderProcedure, false).ConfigureAwait(false);
                }
            }
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    
}