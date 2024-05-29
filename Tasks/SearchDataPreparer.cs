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
        return await searchDataRepository.IsEmpty().ConfigureAwait(false) ||
               await searchDataPointRepository.IsEmpty().ConfigureAwait(false);
    }

    public async Task Execute(IJobExecutionContext context)
    {
        ILookup<string, SearchDataPointModel> officialDataPoints;
        var stopwatch = Stopwatch.StartNew();
        var lastSuccessfulRunDate = await taskExecutionHistoryRepository
            .GetTaskLastSuccessfulRunDate(TaskNameConstants.SearchDataPreparer).ConfigureAwait(false);

        if (await NoDataExists().ConfigureAwait(false))
        {
            Console.WriteLine("Now fetching official data points from beginning of time");
            officialDataPoints = await officialDataPointsRetriever.FetchAllDataPoints(null).ConfigureAwait(false);
        }
        else
        {
            Console.WriteLine($"Now fetching official data points from the: {lastSuccessfulRunDate!.Value.Date}");
            officialDataPoints = await officialDataPointsRetriever.FetchAllDataPoints(lastSuccessfulRunDate)
                .ConfigureAwait(false);
        }
        Console.WriteLine("Fetch done");
        Console.WriteLine("Now processing official data points...");
        var processedOfficialItems =
            await CreateDataPoints(context, officialDataPoints, false, true).ConfigureAwait(false);
        Console.WriteLine("Now processing user supplied data points...");

        Console.WriteLine("Now calculating min and max pricing for all data points.");
        CalculateMinAndMaxPrices(processedOfficialItems);
        
        Console.WriteLine("Populating search data table....");
        await InsertSearchData(processedOfficialItems.SearchData).ConfigureAwait(false);
        await InsertSearchDataPoints(processedOfficialItems.SearchDataPoints).ConfigureAwait(false);

        stopwatch.Stop();
        Console.Clear();
        Console.WriteLine(
            $"Task Completed! Task took: {stopwatch.Elapsed.Hours} hours {stopwatch.Elapsed.Minutes} minutes and {stopwatch.Elapsed.Seconds} seconds");
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

        ArgumentNullException.ThrowIfNull(procedures, nameof(procedures));
        foreach (var item in officialDataPoints) //tariff code groupings
        {
            //for each tariff code, there are numerous data points. Try group them further by medical aid scheme
            var medicalAidGroupings = item.ToLookup(key => key.MedicalAidSchemeId, value => value);

            foreach (var medicalAid in medicalAidGroupings)
            {
                foreach (var dataPoint in medicalAid)
                {
                    //go through all the data points for the provider
                    SearchData? searchData = null;
                    if (!searchDataItems.Exists(x =>
                            string.Equals(x.MedicalAidSchemeId, dataPoint.MedicalAidSchemeId,
                                StringComparison.OrdinalIgnoreCase)
                            && string.Equals(x.TariffCode, dataPoint.TariffCode, StringComparison.OrdinalIgnoreCase)))
                    {
                        searchData = new SearchData
                        {
                            Id = Guid.NewGuid().ToString(),
                            CreatedDate = DateTime.Now,
                            MedicalAidSchemeId = dataPoint.MedicalAidSchemeId,
                            HasOfficialDataPoints = isOfficialSupplied,
                            HasUserDataPoints = isUserSupplied,
                            TariffCode = dataPoint.TariffCode,
                            UpdateDate = DateTime.Now,
                            TariffCodeDescription = procedures.First(x =>
                                    string.Equals(x.Code, dataPoint.TariffCode, StringComparison.OrdinalIgnoreCase))
                                .CodeDescriptor,
                        };
                        searchDataItems.Add(searchData);
                    }
                    else
                    {
                        searchData = searchDataItems.First(x =>
                            string.Equals(x.TariffCode, dataPoint.TariffCode, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(x.MedicalAidSchemeId, dataPoint.MedicalAidSchemeId,
                                StringComparison.OrdinalIgnoreCase));
                        searchData.HasOfficialDataPoints = !isUserSupplied;
                        searchData.UpdateDate = DateTime.Now;
                        searchData.HasUserDataPoints = isUserSupplied || dataPoint.IsUserDataPoint;
                    }

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
        }
        
        return new()
        {
            SearchData = searchDataItems,
            SearchDataPoints = searchDataPoints,
        };
    }

    // it could be argued that this calculation could be done in the above method, however, some strange calculations mismatches happened,
    // and I am too lazy to fine-tune the code :-) . My code has consumed more electricity than necessary (send the environmentalists my way :-) ) 
    private void CalculateMinAndMaxPrices(CreateDataPointsModel dataPointsModel)
    {
        Console.WriteLine("Now calculating min and maximum prices.");

        foreach (var searchData in dataPointsModel.SearchData)
        {
            var searchDataPoints = dataPointsModel.SearchDataPoints.Where(x => x.SearchDataId == searchData.Id);
            var maxYearToAggregateFor = searchDataPoints.Select(x => x.YearValidFor).DefaultIfEmpty().Max();

            var maxPrice = searchDataPoints.Where(x => x.YearValidFor == maxYearToAggregateFor)
                .Select(x => x.Price)
                .DefaultIfEmpty(0)
                .Max();
            var minPrice = searchDataPoints.Where(x => x.YearValidFor == maxYearToAggregateFor)
                .Select(x => x.Price)
                .DefaultIfEmpty(0)
                .Min();

            if (minPrice is 0 && maxPrice is 0)
            {
                maxPrice = searchDataPoints
                    .Select(x => x.Price)
                    .DefaultIfEmpty(0)
                    .Max();
                minPrice = searchDataPoints
                    .Select(x => x.Price)
                    .DefaultIfEmpty(0)
                    .Min();
            }

            searchData.StartPrice = minPrice;
            searchData.EndPrice = maxPrice;
            searchData.UpdateDate = DateTime.Now;
        }
        
        Console.WriteLine("Calculations complete.");
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