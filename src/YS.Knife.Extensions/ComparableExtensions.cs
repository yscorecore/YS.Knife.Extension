namespace System
{
    public static class ComparableExtensions
    {
        public static bool IsBetween<T>(this T value, T start, T end)
         where T : IComparable<T>
        {
            return value.CompareTo(start) >= 0 && value.CompareTo(end) <= 0;
        }
        public static bool IsNotBetween<T>(this T value, T start, T end)
         where T : IComparable<T>
        {
            return !value.IsBetween(start, end);
        }
    }
}
