using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool;

public sealed class ProviderProcedureTypeInitializer
{
    private readonly IProviderProcedureTypeRepository _providerProcedureTypeRepository;
    private readonly MediGuruDbContext _dbContext;

    public ProviderProcedureTypeInitializer(IProviderProcedureTypeRepository providerProcedureTypeRepository,
        MediGuruDbContext dbContext)
    {
        _providerProcedureTypeRepository = providerProcedureTypeRepository;
        _dbContext = dbContext;
    }

    public async Task ProcessAsync()
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            await _providerProcedureTypeRepository.InsertAsync(new ProviderProcedureType
            {
                Name = "Contracted Medical Practitioners"
            });
            await _providerProcedureTypeRepository.InsertAsync(new ProviderProcedureType
            {
                Name = "Non-Contracted Medical Practitioners"
            });
            await transaction.CommitAsync();
            Console.WriteLine("Provider Procedure Types added successfully!");
        }).ConfigureAwait(false);
    }
}