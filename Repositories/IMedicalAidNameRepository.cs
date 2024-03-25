using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.Repositories;

public interface IMedicalAidNameRepository
{
    Task InsertBulk(List<string> medicalAidNames);
    
    Task<List<MedicalAidNameDetail>> FetchAll();
}