using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuru.DataExtractionTool.DatabaseModels;

public sealed class TaskExecutionHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    public bool? Success { get; set; }
    
    [Required]
    public DateTime DateAdded { get; set; }
    
    [Required]
    public DateTime ExecutionStartTime { get; set; }
    
    public DateTime? ExecutionEndTime { get; set; }
    
    public bool? Vetoed { get; set; }
    
    [MaxLength(40000)]
    public string? JobData { get; set; }
    
    [MaxLength(40000)]
    public string? ExceptionDetails { get; set; }
}