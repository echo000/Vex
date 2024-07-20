using System;
using System.Collections.Generic;

namespace PhilLibX
{
    /// <summary>
    /// A class with Enumerable Extensions
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, TSource start, Func<TSource, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            foreach (TSource element in source)
                if (predicate(element)) return element;

            return start;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultVal, Func<TSource, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            TSource result = defaultVal;

            foreach (TSource element in source)
            {
                if (predicate(element))
                {
                    result = element;
                }
            }
            return result;
        }
    }
}
