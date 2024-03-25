using System.Diagnostics;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;
using Quartz;
using SearchData = MediGuru.DataExtractionTool.DatabaseModels.SearchData;

namespace MediGuru.DataExtractionTool.Tasks;

public sealed class SearchDataPreparer(
    MediGuruDbContext dbContext,
    ITaskExecutionHistoryRepository taskExecutionHistoryRepository,
    IProviderProcedureDataPointsRetriever officialDataPointsRetriever,
    ISearchDataRepository searchDataRepository,
    IProcedureRepository procedureRepository,
    ISearchDataPointRepository searchDataPointRepository)
    : IJob
{
    private async Task<bool> NoDataExists()
    {
        return await searchDataRepository.IsEmpty() || await searchDataPointRepository.IsEmpty();
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        ILookup<string, SearchDataPointModel> officialDataPoints;
        var stopwatch = Stopwatch.StartNew();
        var lastSuccessfulRunDate = await taskExecutionHistoryRepository
            .GetTaskLastSuccessfulRunDate(TaskNameConstants.SearchDataPreparer).ConfigureAwait(false);

        if (await NoDataExists().ConfigureAwait(false))
        {
            officialDataPoints = await officialDataPointsRetriever.FetchAllDataPoints(null).ConfigureAwait(false);
        }
        else
        {
            officialDataPoints = await officialDataPointsRetriever.FetchAllDataPoints(lastSuccessfulRunDate).ConfigureAwait(false);
        }
        
        Console.WriteLine("Now processing official data points...");
        var processedOfficialItems = await CreateDataPoints(context, officialDataPoints, false, true).ConfigureAwait(false);

        Console.WriteLine("Populating search data table....");
        await InsertSearchData(processedOfficialItems.SearchData).ConfigureAwait(false);
        await InsertSearchDataPoints(processedOfficialItems.SearchDataPoints).ConfigureAwait(false);
        
        stopwatch.Stop();
        Console.Clear();
        Console.WriteLine($"Task Completed! Task took: {stopwatch.Elapsed.Hours} hours {stopwatch.Elapsed.Minutes} minutes and {stopwatch.Elapsed.Seconds} seconds");
    }

    private async Task<CreateDataPointsModel> CreateDataPoints(IJobExecutionContext context,
        ILookup<string, SearchDataPointModel> officialDataPoints,
        bool isUserSupplied, bool isOfficialSupplied)
    {
        var searchDataItems = new List<SearchData>();
        var searchDataPoints = new List<SearchDataPoint>();
        List<Procedure> procedures = null;

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            var i = await procedureRepository.FetchAll().ConfigureAwait(false);
            procedures = i.ToList();
        }).ConfigureAwait(false);
        
        ArgumentNullException.ThrowIfNull(procedures, "procedures");
        foreach (var item in officialDataPoints)
        {
            var yearToAggregate = item.Max(x => x.YearValidFor);
            var sortedItems = item.OrderByDescending(x => x.YearValidFor);
            var minPrice = sortedItems.Where(x => x.YearValidFor == yearToAggregate).Min(x => x.Price);
            var maxPrice = sortedItems.Where(x => x.YearValidFor == yearToAggregate).Max(x => x.Price);
            
            foreach (var dataPoint in sortedItems)
            {
                //do not index items which do not have a price; GEMS will not pay for these.
                if (dataPoint.Price <= 0)
                {
                    continue;
                }
                
                SearchData? searchData = null;
                if (!searchDataItems.Exists(x => string.Equals(x.MedicalAidSchemeId, dataPoint.MedicalAidSchemeId, StringComparison.OrdinalIgnoreCase) && string.Equals(x.TariffCode, dataPoint.TariffCode, StringComparison.OrdinalIgnoreCase)))
                {
                    searchData = new SearchData
                    {
                        Id = Guid.NewGuid().ToString(),
                        CreatedDate = DateTime.Now,
                        MedicalAidSchemeId = dataPoint.MedicalAidSchemeId,
                        HasOfficialDataPoints = isOfficialSupplied,
                        StartPrice = minPrice,
                        EndPrice = maxPrice,
                        HasUserDataPoints = isUserSupplied,
                        TariffCode = dataPoint.TariffCode,
                        UpdateDate = DateTime.Now,
                        YearValidFor = dataPoint.YearValidFor,
                        TariffCodeDescription = procedures.First(x =>
                                string.Equals(x.Code, dataPoint.TariffCode,
                                    StringComparison.OrdinalIgnoreCase))
                            .CodeDescriptor
                    };
                    searchDataItems.Add(searchData);
                }
                else
                {
                    searchData = searchDataItems.First(x => string.Equals(x.TariffCode, dataPoint.TariffCode, StringComparison.OrdinalIgnoreCase) && string.Equals(x.MedicalAidSchemeId, dataPoint.MedicalAidSchemeId, StringComparison.OrdinalIgnoreCase));
                    searchData.HasOfficialDataPoints = !isUserSupplied;
                }
                
                var updateDate = DateTime.Now;
                searchData.StartPrice = minPrice;
                searchData.EndPrice = maxPrice;
                searchData.UpdateDate = updateDate;

                var searchDataPointItem = new SearchDataPoint
                {
                    Id = Guid.NewGuid().ToString(),
                    CategoryId = dataPoint.CategoryId,
                    DateAdded = dataPoint.DateAdded,
                    DisciplineId = dataPoint.DoctorId,
                    IsOfficialSource = dataPoint.IsOfficialDataPoint,
                    IsUserSupplied = dataPoint.IsUserDataPoint,
                    IsThirdPartySource = dataPoint.IsThirdPartyDataPoint,
                    SearchDataId = searchData.Id,
                    MedicalAidSchemeId = dataPoint.MedicalAidSchemeId,
                    MedicalAidPlanName = dataPoint.MedicalAidPlanOption,
                    Price = dataPoint.Price,
                    YearValidFor = dataPoint.YearValidFor,
                };
                searchDataPoints.Add(searchDataPointItem);
            }
        }

        return new()
        {
            SearchData = searchDataItems,
            SearchDataPoints = searchDataPoints,
        };
    }

    private async Task InsertSearchData(List<SearchData> searchData)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            dbContext.SearchDatas.AddRange(searchData);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private async Task InsertSearchDataPoints(List<SearchDataPoint> searchDataPoints)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            dbContext.SearchDataPoints.AddRange(searchDataPoints);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
    
    private class CreateDataPointsModel
    {
        public List<SearchData> SearchData { get; set; }
        
        public List<SearchDataPoint> SearchDataPoints { get; set; }
    }
}