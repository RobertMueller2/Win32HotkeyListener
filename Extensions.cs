using System;
using System.Collections.Generic;

//TODO: one of these days, this might get its own lib
namespace Win32HotkeyListener {

    public static class Extensions {

        public static int Clamp(this int baseValue, int min, int max) => Math.Min(Math.Max(baseValue, min), max);

        public static int IncOrDec(this int baseValue, int increment, bool increase) => increase ? baseValue + increment : baseValue - increment;

        public static string Coalesce(this string baseValue, string fallback) => string.IsNullOrEmpty(baseValue) ? fallback : baseValue;

        public static string ReplaceEnvVars(this string s) => Environment.ExpandEnvironmentVariables(s);

        public static List<T> Coalesce<T>(this List<T> baseValues, T fallback) => baseValues == null || baseValues.Count == 0 ? new List<T> { fallback } : baseValues;
    }

}
