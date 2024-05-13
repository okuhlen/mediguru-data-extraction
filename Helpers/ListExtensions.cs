namespace MediGuru.DataExtractionTool.Helpers;

public static class ListExtensions
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items,
        int maxItems)
    {
        return items.Select((item, inx) => new { item, inx })
            .GroupBy(x => x.inx / maxItems)
            .Select(g => g.Select(x => x.item));
    }
    
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        if(enumerable == null)
            return true;

        return !enumerable.Any();
    }
}