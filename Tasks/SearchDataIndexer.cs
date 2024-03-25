using Elastic.Clients.Elasticsearch;
using MediGuru.DataExtractionTool.Constants;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MediGuru.DataExtractionTool.Tasks;

public class SearchDataIndexer(
    IOptions<ElasticSearchSetting> elasticSearchSettings,
    IEsSearchDataRepository esSearchDataRepository,
    MediGuruDbContext dbContext)
    : ISearchDataIndexer
{
    private readonly ElasticsearchClient _elasticsearchClient = ElasticSearchClientCreator.Create(elasticSearchSettings.Value.Username,
        elasticSearchSettings.Value.Password, elasticSearchSettings.Value.Host, elasticSearchSettings.Value.Port);

    public async Task UpdateIndex(DateTime? startDate = null)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            var indexExistsResult = await _elasticsearchClient.Indices.ExistsAsync(IndexNameConstants.SearchData)
                .ConfigureAwait(false);
            var itemsToIndex = await esSearchDataRepository.Fetch(startDate).ConfigureAwait(false);
            if (!indexExistsResult.Exists)
            {
                await _elasticsearchClient.Indices.CreateAsync<EsSearchData>(request =>
                    {
                        request.Index(IndexNameConstants.SearchData);
                        request.Mappings(mappings => mappings
                            .Properties(p => p
                                .Keyword(t => t.Categories)
                                .Keyword(t => t.Doctors)
                                .Text(t => t.Id, config => config
                                    .Index(index: true)
                                    .Fielddata(fielddata: true))
                                .IntegerNumber(t => t.TariffCode)
                                .Text(t => t.TariffDescription, config => { config.Fielddata(fielddata: true); })
                                .Text(t => t.MedicalAidSchemeName)
                                .Text(t => t.MedicalAidSchemeId)
                                .Boolean(t => t.HasOfficialDataPoints)
                                .Boolean(t => t.HasUserDataPoints)
                                .DoubleNumber(t => t.StartPrice)
                                .DoubleNumber(t => t.EndPrice)
                                .IntegerNumber(t => t.DoctorCount)
                                .IntegerNumber(t => t.CategoryCount)
                                .Date(t => t.CreatedDate)
                                .Date(t => t.UpdateDate)
                            ));
                    })
                    .ConfigureAwait(false);
            }

            var addOrUpdateResult = await _elasticsearchClient.BulkAsync(b => b
                    .Index(IndexNameConstants.SearchData)
                    .UpdateMany(itemsToIndex.ToList().ConvertAll(SearchDataHelper.ToElasticSearchModel),
                        (ud, d) => ud.Doc(d).DocAsUpsert(true)))
                .ConfigureAwait(false);

            if (!addOrUpdateResult.IsValidResponse)
            {
                if (addOrUpdateResult.ApiCallDetails?.OriginalException != null)
                {
                    throw addOrUpdateResult.ApiCallDetails.OriginalException;
                }

                throw new Exception(
                    "Something went wrong with adding or updating documents to the general search index");
            }
        }).ConfigureAwait(false);
    }
}