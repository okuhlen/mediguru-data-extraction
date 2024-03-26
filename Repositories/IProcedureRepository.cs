using MediGuru.DataExtractionTool.DatabaseModels;

namespace MediGuru.DataExtractionTool.Repositories;

public interface IProcedureRepository
{
    Task InsertAsync(Procedure newOne, bool shouldSaveNow = true);
    Task<Procedure?> FetchByCodeAndCategoryId(string tariffCode, string categoryId);

    Task<IList<Procedure>> FetchAll();
}
