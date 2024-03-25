namespace MediGuru.DataExtractionTool.Models;

public sealed class SearchData
{
    public string Id { get; set; }
    
    public string MedicalAidSchemeId { get; set; }
    
    public string MedicalAidName { get; set; }
    
    public string TariffCode { get; set; }
    
    public string TariffDescription { get; set; }
    
    public bool HasOfficialDataPoints { get; set; }
    
    public bool HasUserDataPoints { get; set; }
    
    public string MedicalAidSchemeName { get; set; }

    public int DoctorsCount => Doctors?.Count ?? 0;

    public int CategoriesCount => Categories?.Count ?? 0;
    
    public double StartPrice { get; set; }
    
    public double EndPrice { get; set; }
    
    public DateTime CreationDate { get; set; }
    
    public DateTime UpdateDate { get; set; }
    public List<SearchDataPointDetail> Categories { get; set; }
    
    public List<SearchDataPointDetail> DataSourceTypes { get; set; }
    
    public List<SearchDataPointDetail> Doctors { get; set; }
}