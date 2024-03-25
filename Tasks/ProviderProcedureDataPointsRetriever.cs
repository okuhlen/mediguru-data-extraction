using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;

namespace MediGuru.DataExtractionTool.Tasks;

public sealed class ProviderProcedureDataPointsRetriever(
    IProviderProcedureRepository providerProcedureRepository,
    IMedicalAidNameRepository medicalAidNameRepository,
    IProviderRepository providerRepository,
    IProcedureRepository procedureRepository)
    : IProviderProcedureDataPointsRetriever
{
    public async Task<ILookup<string, SearchDataPointModel>> FetchAllDataPoints(DateTime? startDate)
    {
        var providerProcedures = await providerProcedureRepository.FetchAll(startDate).ConfigureAwait(false);
        var providers = await providerRepository.FetchAll().ConfigureAwait(false);
        var medicalAids = await medicalAidNameRepository.FetchAll().ConfigureAwait(false);
        var procedures = await procedureRepository.FetchAll().ConfigureAwait(false);
        
        var processedProcedures = new List<(string ProcedureId, SearchDataPointModel Model)>();
        
        foreach (var item in providerProcedures)
        {
            var procedure = procedures.First(x => string.Equals(x.ProcedureId, item.ProcedureId, StringComparison.OrdinalIgnoreCase));
            var provider = providers.First(x => string.Equals(x.ProviderId, item.ProviderId, StringComparison.Ordinal));
            var medicalAidName =
                medicalAids.First(x => string.Equals(x.Name, MedicalAidNameHelper.GetNameFromProcedure(provider.Name), StringComparison.OrdinalIgnoreCase));
            
            processedProcedures.Add((item.ProcedureId, new SearchDataPointModel(item, medicalAidName!.Id, procedure.CategoryId, procedure.Code))!);
        }

        return processedProcedures.ToLookup(key => key.ProcedureId, val => val.Model);
    }
}