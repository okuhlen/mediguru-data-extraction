namespace MediGuru.DataExtractionTool.Helpers;

//very hacky class :-(
public static class MedicalAidNameHelper
{
    public static string GetNameFromProcedure(string name)
    {
        switch (name)
        {
            case "Government Employees Medical Scheme (GEMS)":
                return "GOVERNMENT EMPLOYEES MEDICAL SCHEME (GEMS)";
            
            case "WoolTru Healthcare Fund":
                return "WOOLTRU HEALTHCARE FUND";
            
            case "Momentum Health":
                return "MOMENTUM HEALTH";
            
            default:
                throw new NotSupportedException($"The provided name is not mapped yet: {name}");
        }
    }
}