using System.Globalization;

namespace MediGuru.DataExtractionTool.Helpers;

public static class FormattingHelpers
{
    public static double? GetFormattedWoolTruPrice(string priceString)
    {
        CultureInfo cultureInfo = new CultureInfo("en-ZA");
        string cleanPriceString = priceString.Replace("R", "").Trim();
        cleanPriceString = cleanPriceString.Replace(" ", "");
        cleanPriceString = cleanPriceString.Replace(",", cultureInfo.NumberFormat.CurrencyDecimalSeparator);
        
        // Parse the cleaned price string into a double value
        double price = double.Parse(cleanPriceString, NumberStyles.Currency, cultureInfo);
        
        return price;
    }
    //code taken from: https://stackoverflow.com/questions/1354924/how-do-i-parse-a-string-with-a-decimal-point-to-a-double 
    public static double FormatProcedurePrice(string price)
    {
        price = price.Trim();
        if (string.IsNullOrEmpty(price) || string.IsNullOrWhiteSpace(price))
        {
            return 0;
        }
        
        if (!price.StartsWith("R"))
        {
            if (double.TryParse(price, NumberStyles.Any, CultureInfo.CurrentCulture, out var p1))
            {
                return p1;
            }
            
            if (double.TryParse(price, NumberStyles.Any, CultureInfo.GetCultureInfo("en-ZA"), out var p2))
            {
                return p2;
            }

            if (double.TryParse(price, NumberStyles.Any, CultureInfo.InvariantCulture, out var p3))
            {
                return p3;
            }

            throw new Exception($"Cannot parse the provided input: {price}");
        }
        price = price.Remove(0, 1); //remove the rands price
        price = price.Replace(" ", ""); //remove all the spaces
        if (double.TryParse(price, NumberStyles.Any, CultureInfo.CurrentCulture, out var p4))
        {
            return p4;
        }
            
        if (double.TryParse(price, NumberStyles.Any, CultureInfo.GetCultureInfo("en-ZA"), out var p5))
        {
            return p5;
        }

        if (double.TryParse(price, NumberStyles.Any, CultureInfo.InvariantCulture, out var p6))
        {
            return p6;
        }

        throw new Exception($"Cannot parse the provided input: {price}");
    }
}