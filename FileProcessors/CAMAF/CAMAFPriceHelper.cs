namespace MediGuru.DataExtractionTool.FileProcessors.CAMAF;

internal static class CAMAFPriceHelper
{
    public static double GetPrice(string priceColumnData)
    {
        if (string.IsNullOrEmpty(priceColumnData))
        {
            return 0;
        }

        if (priceColumnData.StartsWith("R"))
        {
            var priceFormatted = priceColumnData.Substring(1, priceColumnData.Length - 1);
            if (double.TryParse(priceFormatted, out var price))
            {
                return price;
            }

            return 0;
        }

        if (priceColumnData.StartsWith("**"))
        {
            var formattedPrice = priceColumnData.Substring(4, 6);
            if (double.TryParse(formattedPrice, out var p))
            {
                return p;
            }

            return 0;
        }
        
        if (priceColumnData.StartsWith("*"))
        {
            var formattedPrice = priceColumnData.Substring(3, 6);
            if (double.TryParse(formattedPrice, out var p3))
            {
                return p3;
            }

            return 0;
        }

        if (double.TryParse(priceColumnData, out var p2))
        {
            return p2;
        }

        throw new NotSupportedException($"The provided price information is not supported for this task: {priceColumnData}");
    }
}