using System;
using System.Collections.Generic;

namespace Refact
{
    public static class Extensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static decimal ToDecimal(this string str, string logPar1, string logPar2, ref bool error)
        {
            if (!decimal.TryParse(str, out decimal result))
            {
                ErrLogger.Error("Unable to parse text to decimal", logPar1, logPar2);
                error = true;
            }
            return result;
        }

        public static int ToInt(this string str, string logPar1, string logPar2, ref bool error)
        {
            if (!int.TryParse(str, out int result))
            {
                ErrLogger.Error("Unable to parse text to integer", logPar1, logPar2);
                error = true;
            }
            return result;
        }
    }
}
