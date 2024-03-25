using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuru.DataExtractionTool.DatabaseModels;

public sealed class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string CategoryId { get; set; }

    [Required]
    [MaxLength(300)]
    public string Description { get; set; }

    [Required]
    public DateTime DateAdded { get; set; }
}
