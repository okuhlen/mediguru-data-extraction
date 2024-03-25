using System.ComponentModel;

namespace MediGuru.DataExtractionTool.Enumerations;

public enum EsDataSourceType
{
    [Description("Official Data Points")]
    OfficialDataPoint,
    
    [Description("User-Supplied Data Points")]
    UserDataPoint,
    
    [Description("Third Party Data Points")]
    ThirdPartyDataPoint
}