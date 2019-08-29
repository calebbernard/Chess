using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Library
{
    static class Extensions
    {
        public static bool Empty<T>(this IEnumerable<T> source)
        {
            return source.Count() == 0;
        }

        public static void Each<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static List<int> Range(this int start, int length)
        {
            var result = new List<int>();
            for (int x = 0; result.Count < length; x++)
            {
                result.Add(start + x);
            }
            return result;
        }
    }
}
