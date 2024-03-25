using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool;

public sealed class ProviderProcedureDataSourceInitializer
{
    private readonly MediGuruDbContext _dbContext;
    private readonly IProviderProcedureDataSourceTypeRepository _sourceTypeRepository;

    public ProviderProcedureDataSourceInitializer(MediGuruDbContext dbContext,
        IProviderProcedureDataSourceTypeRepository sourceTypeRepository)
    {
        _dbContext = dbContext;
        _sourceTypeRepository = sourceTypeRepository;
    }

    public async Task ProcessAsync()
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            await _sourceTypeRepository.InsertAsync(new()
            {
                Name = "HealthMan",
                SourceUrl = "https://www.healthman.co.za/",
                WebsiteUrl = "https://www.healthman.co.za/Tariffs/Tariffs2023"
            });

            await _sourceTypeRepository.InsertAsync(new()
            {
                Name = "Momentum",
                SourceUrl = "https://provider.momentum.co.za/default.aspx?wv0/VQt+350aauRh1kSVjg==",
                WebsiteUrl = "https://momentum.co.za/"
            });

            await _sourceTypeRepository.InsertAsync(new()
            {
                Name = "GEMS",
                SourceUrl = "https://www.gems.gov.za/en/Healthcare-Providers/Tariff-Files/2023-Tariff-Files?year=2023",
                WebsiteUrl = "https://www.gems.gov.za/"
            });

            await _sourceTypeRepository.InsertAsync(new()
            {
                Name = "WoolTru",
                SourceUrl =
                    "https://www.wooltruhealthcarefund.co.za/default.aspx?HEQD9dXSWMgeyCDbKJ5ykDPmfc/VNj+4IM56k3OrknzeeFUNGAfxTax4Qrmw8NCCYMN1s8KEKwJOsp77SbygMw==",
                WebsiteUrl = "https://www.wooltruhealthcarefund.co.za/"
            }).ConfigureAwait(false);

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
}