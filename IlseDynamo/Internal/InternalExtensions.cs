using System;
using System.Collections.Generic;
using System.IO;

using Autodesk.DesignScript.Runtime;

namespace Internal
{
#pragma warning disable CS1591

    /// <summary>
    /// Common extensions.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class InternalExtensions
    {
        /// <summary>
        /// Wraps an array of strings into a set using either case sensitive or insensitive matching.
        /// </summary>
        /// <param name="array">The array</param>
        /// <param name="ignoreCase">Whether to respect case</param>
        /// <returns>A set instance</returns>
        public static ISet<string> ToSet(this IEnumerable<string> array, bool ignoreCase)
        {
            return new HashSet<string>(array, new IgnoreCaseEqualityComparer(ignoreCase));
        }

        /// <summary>
        /// Returns a relative path to the given path if any exists.
        /// </summary>
        /// <param name="fullPath">The full path to be relative</param>
        /// <param name="basePath">To the base path</param>
        /// <returns>A path (relative or absolute)</returns>
        public static string RelativePathOf(this string fullPath, string basePath)
        {
            if (String.IsNullOrEmpty(fullPath))
                return String.Empty;
            if (String.IsNullOrEmpty(basePath))
                return fullPath;

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            if (fullUri.Scheme != baseUri.Scheme)
            {
                return fullPath;
            }

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            return Uri.UnescapeDataString(relativeUri.ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
    }

#pragma warning restore CS1591
}
