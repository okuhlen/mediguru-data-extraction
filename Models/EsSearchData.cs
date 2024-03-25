using MediGuru.DataExtractionTool.Enumerations;

namespace MediGuru.DataExtractionTool.Models;

public class EsSearchData
{
    public string Id { get; set; }
    
    public int TariffCode { get; set; }
    
    public string TariffDescription { get; set; }
    
    public string MedicalAidSchemeName { get; set; }
    
    public string MedicalAidSchemeId { get; set; }
    
    public bool HasOfficialDataPoints { get; set; }
    
    public bool HasUserDataPoints { get; set; }
    
    public double StartPrice { get; set; }
    
    public double EndPrice { get; set; }
    public List<EsDoctorType> Doctors { get; set; }
    public List<EsCategory> Categories { get; set; }

    public int DoctorCount => Doctors?.Count ?? 0;

    public int CategoryCount => Categories?.Count ?? 0;
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime UpdateDate { get; set; }
}