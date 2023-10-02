namespace Api;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class =>
        from item in source
        where item is not null
        select item;

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : struct =>
        from item in source
        where item.HasValue
        select item.Value;
}