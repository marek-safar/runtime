// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

namespace System.Collections
{
    /// <summary>
    /// Designed to support hashtables which require case-insensitive behavior while still maintaining case,
    /// this provides an efficient mechanism for getting the hashcode of the string ignoring case.
    /// </summary>
    [Obsolete("Please use StringComparer instead.")]
    public class CaseInsensitiveHashCodeProvider : IHashCodeProvider
    {
        private static volatile CaseInsensitiveHashCodeProvider? s_invariantCaseInsensitiveHashCodeProvider;
        private readonly CompareInfo _compareInfo;

        public CaseInsensitiveHashCodeProvider()
        {
            _compareInfo = CultureInfo.CurrentCulture.CompareInfo;
        }

        public CaseInsensitiveHashCodeProvider(CultureInfo culture)
        {
            if (culture is null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            _compareInfo = culture.CompareInfo;
        }

        public static CaseInsensitiveHashCodeProvider Default => new CaseInsensitiveHashCodeProvider();

        public static CaseInsensitiveHashCodeProvider DefaultInvariant => s_invariantCaseInsensitiveHashCodeProvider ??
            (s_invariantCaseInsensitiveHashCodeProvider = new CaseInsensitiveHashCodeProvider(CultureInfo.InvariantCulture));

        public int GetHashCode(object obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            string? s = obj as string;
            return s is not null ?
                _compareInfo.GetHashCode(s, CompareOptions.IgnoreCase) :
                obj.GetHashCode();
        }
    }
}
