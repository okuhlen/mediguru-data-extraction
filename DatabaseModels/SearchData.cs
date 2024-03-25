using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediGuru.DataExtractionTool.Models;

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
    
    /// <summary>
    /// This field shows the year at which the data point is valid for. We only want to show the latest start and end price.
    /// IE: do not include 2023, 2022 points here. 
    /// </summary>
    [Required]
    public int YearValidFor { get; set; }
    
    public MedicalAidScheme MedicalAidScheme { get; set; }
}