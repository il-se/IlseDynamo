using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Runtime;

namespace IlseDynamo.Internal
{
#pragma warning disable CS1591

    [IsVisibleInDynamoLibrary(false)]
    public class IgnoreCaseEqualityComparer : IEqualityComparer<string>
    {
        public readonly bool IgnoresCase;

        public IgnoreCaseEqualityComparer(bool ignoreCase)
        {
            IgnoresCase = ignoreCase;
        }

        public bool Equals(string x, string y)
        {
            if (IgnoresCase)
                return EqualityComparer<string>.Default.Equals(x?.ToUpperInvariant(), y?.ToUpperInvariant());
            else
                return EqualityComparer<string>.Default.Equals(x, y);
        }

        public int GetHashCode(string obj)
        {
            if (IgnoresCase)
                return obj.ToUpperInvariant().GetHashCode();
            else
                return obj.GetHashCode();

        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public class DerivedIgnoreCaseEqualityComparer<T> : IEqualityComparer<T>
    {
        public readonly bool IgnoresCase;
        public readonly Func<T, string> Derive;

        public DerivedIgnoreCaseEqualityComparer(Func<T, string> f, bool ignoreCase)
        {
            IgnoresCase = ignoreCase;
            Derive = f;
        }

        public bool Equals(T x, T y)
        {
            if (IgnoresCase)
                return EqualityComparer<string>.Default.Equals(Derive(x)?.ToUpperInvariant(), Derive(y)?.ToUpperInvariant());
            else
                return EqualityComparer<string>.Default.Equals(Derive(x), Derive(y));
        }

        public int GetHashCode(T obj)
        {
            if (IgnoresCase)
                return Derive(obj).ToUpperInvariant().GetHashCode();
            else
                return Derive(obj).GetHashCode();

        }
    }

#pragma warning restore CS1591
}
