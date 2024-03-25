using System.ComponentModel;
using System.Reflection;

namespace MediGuru.DataExtractionTool.Helpers;

//source: https://stackoverflow.com/questions/23563960/how-to-get-enum-value-by-string-or-int 
public static class EnumerationExtensions
{
    public static T ToEnumFromDescriptionAttribute<T>(this string value)
    {
        foreach(var field in typeof(T).GetFields())
        {
            if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (string.Equals(attribute.Description, value, StringComparison.OrdinalIgnoreCase))
                    return (T)field.GetValue(null);
            }
            else
            {
                if (string.Equals(field.Name, value, StringComparison.OrdinalIgnoreCase))
                    return (T)field.GetValue(null);
            }
        }
        
        throw new ArgumentException("Not found.", nameof(value));
    }
    
    public static T ToEnum<T>(this int param) where T: Enum
    {
        var info = typeof(T);
        T result = (T)Enum.Parse(typeof(T), param.ToString(), true);
        return result;
    }
    
    public static T ParseEnum<T>(string value) where T : Enum
    {
        return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
    }
    
    public static string GetDescription<T>(this T source)
    {
        FieldInfo fi = source.GetType().GetField(source.ToString());

        DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
            typeof(DescriptionAttribute), false);

        if (attributes != null && attributes.Length > 0) return attributes[0].Description;
        else return source.ToString();
    }
}