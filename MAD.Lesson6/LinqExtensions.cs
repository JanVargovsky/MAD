using System;
using System.Collections.Generic;
using System.Linq;

namespace MAD.Lesson6
{
    public static class LinqExtensions
    {
        public static TKey Median<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var collection = source.OrderBy(keySelector).ToList();
            return keySelector(collection[collection.Count / 2]);
        }

        public static TSource MedianBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var collection = source.OrderBy(keySelector).ToList();
            return collection[collection.Count / 2];
        }

        /// <summary>
        /// Vrati rozptyl
        /// </summary>
        public static float Variance<TSource>(this IList<TSource> source, float mean, Func<TSource, float> keySelector)
        {
            return (float)((1d / source.Count) * source.Sum(t => Math.Pow(keySelector(t) - mean, 2)));
        }
    }
}
