using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.Repositories;

public interface IProviderProcedureTypeRepository
{
    Task InsertAsync(ProviderProcedureType newType);
}