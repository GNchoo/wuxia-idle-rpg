using System.Collections.Generic;

namespace BreakInfinity
{
    public static class LinqExtentions
    {
        public static BigDouble Sum(this IEnumerable<BigDouble> collection)
        {
            var result = new BigDouble();
            foreach (var element in collection)
            {
                result += element;
            }
            return result;
        }
    }
}
