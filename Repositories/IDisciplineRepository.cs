using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.Repositories;

public interface IDisciplineRepository
{
    Task InsertAsync(Discipline newOne, bool shouldSaveNow = true);

    Task<Discipline> FetchByCode(string code);

    Task<Discipline> FetchByCodeAndSubCode(string code, string subCode);

    Task<bool> Exists(string code);
    Task<bool> Exists(string code, string subCode);

    Task<Discipline?> FetchByName(string name);

    Task<List<Discipline>> FetchAll();

    Task<Discipline> FetchById(string id);
}
