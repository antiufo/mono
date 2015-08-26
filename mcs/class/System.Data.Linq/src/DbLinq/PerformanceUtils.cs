using DbLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq
{
    internal static partial class FastLinqExtensions
    {
        public static List<TResult> SelectToList<T, TResult>(this IList<T> source, Func<T, TResult> selector)
        {
            var count = source.Count;
            if (count == 0) return new List<TResult>(0);
            var l = new List<TResult>(count);
            for (int i = 0; i < count; i++)
            {
                l.Add(selector(source[i]));
            }
            return l;
        }
        public static TResult[] SelectToArray<T, TResult>(this IList<T> source, Func<T, int, TResult> selector)
        {
            var count = source.Count;
            if (count == 0) return EmptyArray<TResult>.Instance;
            var l = new TResult[count];
            for (int i = 0; i < count; i++)
            {
                l[i] = selector(source[i], i);
            }
            return l;
        }
        public static TResult[] SelectToArray<T, TResult>(this IList<T> source, Func<T, TResult> selector)
        {
            var count = source.Count;
            if (count == 0) return EmptyArray<TResult>.Instance;
            var l = new TResult[count];
            for (int i = 0; i < count; i++)
            {
                l[i] = selector(source[i]);
            }
            return l;
        }

        public static TResult[] SelectToArray<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, int count)
        {

            if (count == 0) return EmptyArray<TResult>.Instance;
            var l = new TResult[count];
            var i = 0;
            foreach (var item in source)
            {
                l[i] = selector(item);
                i++;
            }
            if (i != count) throw new ArgumentException("Incorrect length was provided to SelectoToArray().");
            return l;
        }
        public static List<TResult> SelectToList<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, int count)
        {
            var l = new List<TResult>(count);
            if (count == 0) return l;
            var i = 0;
            foreach (var item in source)
            {
                l.Add(selector(item));
                i++;
            }
            if (i != count) throw new ArgumentException("Incorrect length was provided to SelectoToList().");
            return l;
        }
    }
}

namespace DbLinq
{


    internal class EmptyArray<T>
    {
        public readonly static T[] Instance = new T[0];
    }


}
