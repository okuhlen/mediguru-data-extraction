using MediGuru.DataExtractionTool.DatabaseModels;

namespace MediGuru.DataExtractionTool.Models;

public sealed class SearchDataPointModel
{
    public string DoctorId { get; set; }
    
    public string CategoryId { get; set; }
    
    public bool IsUserDataPoint { get; set; }
    
    public bool IsOfficialDataPoint { get; set; }
    
    public bool IsThirdPartyDataPoint { get; set; }
    
    public double Price { get; set; }
    
    public string MedicalAidPlanOption { get; set; }
    
    public string MedicalAidSchemeId { get; set; }
    
    public DateTime DateAdded { get; set; }
    
    public int YearValidFor { get; set; }
    
    public string TariffCode { get; set; }
    public SearchDataPointModel() { }

    public SearchDataPointModel(MedicalAidSchemeProcedure procedure, string tariffCode)
    {
        DoctorId = procedure.DisciplineId;
        DateAdded = procedure.DateAdded;
        CategoryId = procedure.CategoryId;
        IsUserDataPoint = true; //this is always true for medical aid scheme procedure data
        IsThirdPartyDataPoint = false; //always false in this case
        IsOfficialDataPoint = false;
        Price = procedure.Price;
        MedicalAidPlanOption = procedure.PlanOption;
        MedicalAidSchemeId = procedure.MedicalAidSchemeId;
        YearValidFor = procedure.YearValidFor;
        TariffCode = tariffCode;
    }

    public SearchDataPointModel(ProviderProcedure procedure, string medicalAidSchemeId, string categoryId, string tariffCode)
    {
        DoctorId = procedure.DisciplineId;
        DateAdded = procedure.DateAdded;
        CategoryId = categoryId;
        IsUserDataPoint = false;
        IsThirdPartyDataPoint = false;
        IsOfficialDataPoint = true; //Always true
        Price = procedure.Price ?? 0;
        MedicalAidSchemeId = medicalAidSchemeId;
        YearValidFor = procedure.YearValidFor;
        TariffCode = tariffCode;
    }
}