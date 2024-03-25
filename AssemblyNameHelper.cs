using System.Reflection;

namespace MediGuru.DataExtractionTool;

internal static class AssemblyNameHelper
{
    public static Assembly GetAssemblyName()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            if (assembly.GetName().Name.Contains("DataAccessLayer"))
            {
                return assembly;
            }
        }

        throw new Exception("Could not get a reference to the DAL project");
    }
}
