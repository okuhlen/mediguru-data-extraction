namespace MediGuru.DataExtractionTool.Models;

public sealed class ProcessFileParameters
{
    public int YearValidFor { get; set; }
    
    public int StartingRow { get; set; }
    
    public string CategoryName { get; set; }
    
    public string FileLocation { get; set; }
    
    public string? AdditionalNotes { get; set; }
    
    public int? EndingRow { get; set; }
    
    public bool? IsNonContracted { get; set; }
    
    public bool? IsContracted { get; set; }
    public IList<int> RowsToSkip { get; set; }
}