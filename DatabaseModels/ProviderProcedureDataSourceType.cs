using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuru.DataExtractionTool.DatabaseModels;

/// <summary>
/// This table will let us know where the data originated from (official healthcare provider site, or 3rd party report)
/// </summary>
///
public sealed class ProviderProcedureDataSourceType
{
    //this table will be important for keeping 3rd party sources of data. 
    //we would like to inform the user for cases where we do have data for other providers, but do not have data from official sources (like Momentum, WoolTru and GEMS). 
    //example: https://www.healthman.co.za/Tariffs/Tariffs2023
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string ProviderProcedureDataSourceTypeId { get; set; }
    [Required]
    public string Name { get; set; } //name of 3rd party source
    [Required]
    public string WebsiteUrl { get; set; }
    public string? SourceUrl { get; set; } //source for the material.
    public bool IsOfficialSource { get; set; } //true if taken from provider site, false if taken from 3rd party site (eg: healthman)
    
    public bool IsUserSupplied { get; set; }
}
