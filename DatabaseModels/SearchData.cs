using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuru.DataExtractionTool.DatabaseModels;

public sealed class SearchData
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public string Id { get; set; }
    
    [ForeignKey("MedicalAidSchemeId")]
    [Required]
    public string MedicalAidSchemeId { get; set; }
    
    [Required]
    public bool HasOfficialDataPoints { get; set; }
    
    [Required]
    public bool HasUserDataPoints { get; set; }
    
    public double StartPrice { get; set; }
    
    public double EndPrice { get; set; }
    
    [Required]
    public string TariffCode { get; set; }
    
    [Required]
    public string TariffCodeDescription { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; }
    
    [Required]
    public DateTime UpdateDate { get; set; }
    
    public MedicalAidScheme MedicalAidScheme { get; set; }
}