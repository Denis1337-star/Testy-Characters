using System;
using UnityEngine;

public static class NumberFormatter 
{
    public static string Format(double value)
    {
        if (value < 0d)
            value = 0d;

        if (value < 1000d)
            return value.ToString("0.##");

        if (value < 1_000_000d)
            return (value / 1000d).ToString("0.##") + "K";

        if (value < 1_000_000_000d)
            return (value / 1_000_000d).ToString("0.##") + "M";

        if (value < 1_000_000_000_000d)
            return (value / 1_000_000_000d).ToString("0.##") + "B";

        return (value / 1_000_000_000_000d).ToString("0.##") + "T";
    }
}
