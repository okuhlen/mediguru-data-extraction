using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors.GEMS;

public sealed class BiokineticsFileProcessor
{
    private readonly MediGuruDbContext _dbContext;
    private readonly IProviderProcedureDataSourceTypeRepository _sourceTypeRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IDisciplineRepository _disciplineRepository;
    private readonly IProcedureRepository _procedureRepository;
    private const string DisciplineCode = "91";
    private readonly IProviderProcedureRepository _providerProcedureRepository;

    public BiokineticsFileProcessor(MediGuruDbContext dbContext,
        IProviderProcedureDataSourceTypeRepository sourceTypeRepository,
        ICategoryRepository categoryRepository,
        IProviderRepository providerRepository,
        IDisciplineRepository disciplineRepository,
        IProcedureRepository procedureRepository,
        IProviderProcedureRepository providerProcedureRepository)
    {
        _dbContext = dbContext;
        _sourceTypeRepository = sourceTypeRepository;
        _categoryRepository = categoryRepository;
        _providerRepository = providerRepository;
        _disciplineRepository = disciplineRepository;
        _procedureRepository = procedureRepository;
        _providerProcedureRepository = providerProcedureRepository;
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

            using var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            var dataSource = await _sourceTypeRepository.FetchByNameAsync("GEMS").ConfigureAwait(false);
            if (dataSource is null)
                throw new Exception($"Missing data source name: GEMS");

            var category = await _categoryRepository.FetchByName(parameters.CategoryName).ConfigureAwait(false);
            var provider = await _providerRepository.FetchByName("Government Employees Medical Scheme (GEMS)")
                .ConfigureAwait(false);
            var discipline = await _disciplineRepository.FetchByCode(DisciplineCode).ConfigureAwait(false);
            if (discipline is null)
            {
                discipline = new Discipline
                {
                    Description = "Biokinetics",
                    Code = DisciplineCode,
                    DateAdded = DateTime.Now,
                    SubCode = "0",
                };
                await _disciplineRepository.InsertAsync(discipline, false).ConfigureAwait(false);
            }

            if (category == null)
            {
                category = new Category
                {
                    Description = parameters.CategoryName,
                    DateAdded = DateTime.Now
                };
                await _categoryRepository.InsertAsync(category, false).ConfigureAwait(false);
            }

            var sheet = document.Worksheets.First();
            foreach (var row in sheet.Rows())
            {
                if (row.RowNumber() < parameters.StartingRow)
                {
                    continue;
                }

                if (row.Cell("A").IsEmpty() || row.Cell("C").IsEmpty())
                    continue;

                var tariffCodeText = row.Cell("A").GetString().Trim();
                if (string.IsNullOrEmpty(tariffCodeText) || string.IsNullOrWhiteSpace(tariffCodeText))
                {
                    continue;
                }

                var procedure = await _procedureRepository.FetchByCodeAndCategoryId(tariffCodeText, category.CategoryId)
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
                    await _procedureRepository.InsertAsync(procedure, false).ConfigureAwait(false);
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
                await _providerProcedureRepository.InsertAsync(providerProcedure, false).ConfigureAwait(false);
            }

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
            Console.WriteLine($"DONE PROCESSING FILE: {parameters.FileLocation}");
        }).ConfigureAwait(false);
    }
}