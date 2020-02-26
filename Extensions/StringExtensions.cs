using System;
using System.Collections.Generic;

namespace Extensions.StringExtensions
{

    public static class StringExtensions
    {

        public static IEnumerable<string> SplitByChunks(this string stringToSplit, int chunkMaxSize)
        {
            for (int i = 0; i < stringToSplit.Length; i += chunkMaxSize)
            {
                yield return stringToSplit.Substring(i, Math.Min(chunkMaxSize, stringToSplit.Length - i));
            }
        }

    }

}