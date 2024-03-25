using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuru.DataExtractionTool.DatabaseModels;

public class MedicalAidScheme
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string MedicalAidSchemeId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public DateTime DateAdded { get; set; }
}