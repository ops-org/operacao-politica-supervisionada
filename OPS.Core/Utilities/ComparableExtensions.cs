using System;

namespace OPS.Core.Utilities;

public static class ComparableExtensions
{
    public static bool IsBetween<T>(this T value, T min, T max) where T : IComparable<T>
    {
        return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
    }

    public static bool IsBetween<T>(this T value, T? min, T? max) where T : struct, IComparable<T>
    {
        if (min.HasValue && value.CompareTo(min.Value) < 0) return false;
        if (max.HasValue && value.CompareTo(max.Value) > 0) return false;
        return true;
    }
}
