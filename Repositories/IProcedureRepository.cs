using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.Repositories;

public interface IProcedureRepository
{
    Task InsertAsync(Procedure newOne, bool shouldSaveNow = true);
    Task<Procedure?> FindByCodeAsync(string code); //this might not be valid! a procedure could span across multiple categories :( whoopsie.
    Task<bool> ExistsByCodeAndCategoryId(string tariffCode, string categoryId);
    Task<Procedure?> FetchByCodeAndCategoryId(string tariffCode, string categoryId);
    Task<int> Count();

    Task<IList<Procedure>> FetchAll();

    Task<bool> ExistsById(string id);

    Task<Procedure> FetchById(string id);

    Task<IList<Procedure>> FetchByCategoryId(string categoryId);
}
