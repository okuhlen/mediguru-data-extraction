using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool;

internal sealed class ProviderInitializer
{
    private readonly IProviderRepository _providerRepository;
    private readonly MediGuruDbContext _dbContext;

    public ProviderInitializer(MediGuruDbContext dbContext, IProviderRepository providerRepository)
    {
        _dbContext = dbContext;
        _providerRepository = providerRepository;
    }

    public async Task ProcessAsync()
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            //todo: we need to decide whether cloud storage is needed for this or not. I am against storing any files in the database or as naked files.
            //more work needed to setup cloud provider (i prefer google cloud storage, hahaha)
            await _providerRepository.InsertAsync(new Provider
            {
                IsRestricted = false,
                Name = "Momentum Health",
                AddedDate = DateTime.Now,
                Description =
                    "Momentum's products and services include financial advice, medical aid, insurance, fiduciary and investment products for individuals and businesses in South Africa.",
                WebsiteUrl = "https://www.momentum.co.za/"
            }).ConfigureAwait(false);

            await _providerRepository.InsertAsync(new Provider
            {
                IsRestricted = true,
                Name = "WoolTru Healthcare Fund",
                Description =
                    "The Wooltru Healthcare Fund is a registered, closed medical scheme in terms of the Medical Schemes Act 131 of 1998.",
                AddedDate = DateTime.Now,
                WebsiteUrl = "https://www.wooltruhealthcarefund.co.za/"
            }).ConfigureAwait(false);

            await _providerRepository.InsertAsync(new Provider
            {
                IsRestricted = true,
                Name = "Government Employees Medical Scheme (GEMS)",
                Description =
                    "GEMS was registered on 1 January 2005 specifically to meet the healthcare needs of government employees. Our goal is to help public service employees and their families to get the best possible healthcare at the most affordable rate.",
                AddedDate = DateTime.Now,
                WebsiteUrl = "https://www.gems.gov.za/"
            }).ConfigureAwait(false);

            await _providerRepository.InsertAsync(new Provider
            {
                IsRestricted = null,
                IsGovernmentBaselineProvider = true,
                Name = "Department of Health Tariff Codes and Prices",
                Description = "Department of Health Tariff Codes and prices laid out by the national government",
                WebsiteUrl = "http://www.gpwonline.co.za/GPWGazettes.htm"
            }).ConfigureAwait(false);

            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
}