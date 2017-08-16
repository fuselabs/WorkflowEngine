// Copyright (c) Microsoft Corporation. All rights reserved.

namespace WorkflowEngine.Utils
{
    public static class HashUtils
    {
        /// <summary>
        /// Nifty utility for combining hash codes in a safe way
        /// (well, as safe as 32 bit hashes can go anyway)
        /// </summary>
        public static int CombineHashCodes(params int[] hashCodes)
        {
            var hash1 = (5381 << 16) + 5381;
            var hash2 = hash1;

            var i = 0;
            foreach (var hashCode in hashCodes)
            {
                if (i % 2 == 0)
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ hashCode;
                else
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ hashCode;

                ++i;
            }
            return hash1 + (hash2 * 1566083941);
        }
    }
}