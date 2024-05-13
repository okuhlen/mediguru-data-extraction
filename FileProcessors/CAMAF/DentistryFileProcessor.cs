using ClosedXML.Excel;
using MediGuru.DataExtractionTool.Constants;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.CAMAF;

public sealed class DentistryFileProcessor(MediGuruDbContext dbContext,
    IProviderProcedureDataSourceTypeRepository sourceTypeRepository,
    ICategoryRepository categoryRepository,
    IProviderRepository providerRepository,
    IDisciplineRepository disciplineRepository,
    IProcedureRepository procedureRepository,
    IProviderProcedureRepository providerProcedureRepository)
{
    private readonly List<string> _tariffCodes = new()
    {
        "8101",
        "8104"
    };

    public async Task ProcessAsync()
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            using var workbook = new XLWorkbook(CAMAFFileConstants.DentistryConsultationsFile);
            foreach (var row in workbook.Worksheets.First().Rows())
            {
                if (row.RowNumber() <= 4)
                {
                    continue;
                }
                
                var category = await categoryRepository.FetchByName("Dentistry").ConfigureAwait(false);
                var provider = await providerRepository.FetchByName("Chartered Accountants (SA) Medical Aid Fund (CAMAF)").ConfigureAwait(false);
                var discipline = await disciplineRepository.FetchByCode("54").ConfigureAwait(false);
                if (discipline is null)
                {
                    Console.WriteLine("Could not find Discipline: 54");
                    continue;
                }

                foreach (var tariffCode in _tariffCodes)
                {
                    var procedure = await procedureRepository.FetchByCodeAndCategoryId(tariffCode, category.CategoryId)
                        .ConfigureAwait(false);
                    if (procedure is null)
                    {
                        procedure = new Procedure
                        {
                            Code = tariffCode,
                            CodeDescriptor = "Consultation (out of hospital) Dentistry",
                            CreatedDate = DateTime.Now,
                            CategoryId = category.CategoryId,
                        };
                        await procedureRepository.InsertAsync(procedure, false).ConfigureAwait(false);
                    }
                    var providerProcedure = new ProviderProcedure
                    {
                        DateAdded = DateTime.Now,
                        AdditionalNotes = "CAMAF base rate",
                        DisciplineId = discipline.DisciplineId,
                        ProviderId = provider.ProviderId,
                        YearValidFor = 2024,
                        Price = CAMAFPriceHelper.GetPrice(row.Cell("C").GetString()),
                        ProcedureId = procedure.ProcedureId,
                    };

                    var procedure80Percent = new ProviderProcedure
                    {
                        DateAdded = DateTime.Now,
                        AdditionalNotes = "CAMAF 80% of base tariff",
                        DisciplineId = discipline.DisciplineId,
                        ProviderId = provider.ProviderId,
                        YearValidFor = 2024,
                        Price = CAMAFPriceHelper.GetPrice(row.Cell("D").GetString()),
                        ProcedureId = procedure.ProcedureId,
                    };

                    await providerProcedureRepository.InsertAsync(procedure80Percent, false).ConfigureAwait(false);
                    await providerProcedureRepository.InsertAsync(providerProcedure, false).ConfigureAwait(false);
                }
            }
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);

    }
}