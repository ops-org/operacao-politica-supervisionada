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

    public static bool OverlapsWith<T>(this (T? De, T? Ate) range1, T? de2, T? ate2) where T : struct, IComparable<T>
    {
        // Two ranges overlap if the start of one is before or on the end of the other, 
        // AND the end of the first is after or on the start of the second.
        // If PeriodoAte is null, it means it is open-ended (infinity).

        // range1: [de1, ate1]
        // range2: [de2, ate2]

        // If either range starts after the other ends, they don't overlap.
        
        // de1 > ate2 (if ate2 is not null)
        if (range1.De.HasValue && ate2.HasValue && range1.De.Value.CompareTo(ate2.Value) > 0) return false;
        
        // de2 > ate1 (if ate1 is not null)
        if (de2.HasValue && range1.Ate.HasValue && de2.Value.CompareTo(range1.Ate.Value) > 0) return false;

        return true;
    }
}
