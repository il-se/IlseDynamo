using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Internal;
using Allplan.Data;

using Autodesk.DesignScript.Runtime;

namespace Allplan
{
    /// <summary>
    /// Node mapping for an Allplan Attribite Favourite (*.atfanfx).
    /// </summary>
    public class AttributeFavourite
    {
        #region Internal

        internal const string EXTENSION = ".atfanfx";

        private string FileName { get; set; }
        private string BasePath { get; set; }
        private AllplanAttributesContainer Favourite { get; set; }

        internal AttributeFavourite()
        {
        }

        #endregion

        /// <summary>
        /// Imports an Allplan Favourite file.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <param name="basePath">If given, a save will keep folder hierarchy the same as input hierarchy</param>
        /// <returns>A favourite</returns>
        public static AttributeFavourite ByFileName(string fileName, string basePath = null)
        {
            return new AttributeFavourite
            {
                FileName = fileName,
                BasePath = basePath,
                Favourite = AllplanAttributesContainer.ReadFrom(fileName)
            };
        }

        /// <summary>
        /// Returns the base folder name (if set).
        /// </summary>
        public string BaseFolder
        {
            get {
                var baseFolder = BasePath?.Trim() ?? Path.GetDirectoryName(FileName);
                if (!baseFolder.EndsWith($"{Path.DirectorySeparatorChar}"))
                    return $"{baseFolder}{Path.DirectorySeparatorChar}";
                else
                    return $"{baseFolder}";
            }
        }

        /// <summary>
        /// Returns the local folder relative to the base path.
        /// </summary>
        public string Folder
        {
            get {
                var folder = $"{Path.GetDirectoryName(FileName)}{Path.DirectorySeparatorChar}".RelativePathOf(BaseFolder);
                if (string.IsNullOrEmpty(folder) || folder.EndsWith($"{Path.DirectorySeparatorChar}"))
                    return folder.Trim();
                else 
                    return $"{folder}{Path.DirectorySeparatorChar}";
            }
        }

        /// <summary>
        /// Saves this favourite as new XML file (using atfanfx-extension).
        /// </summary>
        /// <param name="basePathName">The file name</param>
        /// <returns>The written file name</returns>
        public string SaveAs(string basePathName)
        {
            var trimmedPath = Path.GetDirectoryName($"{basePathName}{Path.DirectorySeparatorChar}");
            var finalFilePath = $"{trimmedPath}{Path.DirectorySeparatorChar}{Folder}{Name}{EXTENSION}";

            var dirInfo = Directory.CreateDirectory(Path.GetDirectoryName(finalFilePath));
            if (!dirInfo.Exists)
                throw new Exception($"Path {dirInfo.FullName} isn't existing.");

            Favourite.WriteTo(finalFilePath);
            return finalFilePath;
        }

        /// <summary>
        /// Gets the file name (without extension and path).
        /// </summary>        
        public string Name
        {
            get => Path.GetFileNameWithoutExtension(FileName);
        }

        /// <summary>
        /// Creates a new favourite excluding given attributes.
        /// </summary>
        /// <param name="names">The attribute names.</param>
        /// <param name="attributeDefinition">The definition to resolve IFNR against</param>
        /// <returns>A new favourite</returns>
        public AttributeFavourite Exclude(string[] names, AttributeDefinition attributeDefinition)
        {
            var nameSet = new HashSet<string>(names);
            var ifNrSet = attributeDefinition.DefinitionCollection
                .AttributeDefinition
                .Where(a => nameSet.Contains(a.Text.Trim()))
                .Select(a => a.Ifnr)
                .ToArray();

            return new AttributeFavourite
            {
                BasePath = BasePath,
                FileName = FileName,
                Favourite = Favourite?.ExcludeByIfNr(ifNrSet)
            };
        }

        /// <summary>
        /// A new favourite by given LOI and definition mapping
        /// </summary>
        /// <param name="attributeLevel">The LOI</param>
        /// <param name="attributeDefinition">The attribute definition</param>
        /// <returns>A new favourite</returns>
        public static AttributeFavourite ByLevel(AttributeLevel attributeLevel, AttributeDefinition attributeDefinition)
        {
            var nameSet = attributeLevel.Attributes.ToSet(false);
            return new AttributeFavourite
            {
                FileName = $"{attributeLevel.Level}",
                Favourite = AllplanAttributesContainer.Create(
                    attributeDefinition.DefinitionCollection
                    .AttributeDefinition
                    .Where(a => nameSet.Contains(a.Text.Trim())), "2019")
            };
        }

        /// <summary>
        /// Creates a new favourite by given LOI and definition.
        /// </summary>
        /// <param name="attributeLevel">The required minium level</param>
        /// <param name="attributeDefinition">The attribute definition</param>
        /// <returns>A new favourite</returns>
        public AttributeFavourite OfLevel(AttributeLevel attributeLevel, AttributeDefinition attributeDefinition)
        {
            var nameSet = attributeLevel.Attributes.ToSet(false);
            var ifnrSet = attributeDefinition.DefinitionCollection.AttributeDefinition
                .Where(a => nameSet.Contains(a.Text.Trim()))
                .Select(a => a.Ifnr)
                .ToArray();

            return new AttributeFavourite
            {
                FileName = $"{Path.GetDirectoryName(FileName)}{Path.DirectorySeparatorChar}{Name}_{attributeLevel.Level}{EXTENSION}",
                BasePath = BasePath,
                Favourite = Favourite.FilterByIfNr(ifnrSet)
            };
        }

        /// <summary>
        /// Creates a new favourite excluding given attributes IFNR's.
        /// </summary>
        /// <param name="ifnr">The attribute IFNRs.</param>
        /// <returns>A new favourite</returns>
        public AttributeFavourite Exclude(long[] ifnr)
        {
            return new AttributeFavourite
            {
                FileName = FileName,
                BasePath = BasePath,
                Favourite = Favourite?.ExcludeByIfNr(ifnr)
            };
        }

        internal IEnumerable<Tuple<string, string>> ToAttributeValue(AttributeDefinition attributeDefinition)
        {
            var ifnrMap = attributeDefinition
                .DefinitionCollection
                .AttributeDefinition
                .ToDictionary(a => a.Ifnr);

            return Favourite?.AttributeSet
                .Attributes
                .Select(a => new Tuple<string, string>(ifnrMap[a.Ifnr]?.Text.Trim(), a.Value));
        }

        /// <summary>
        /// Dumps the data of favourite using the given defintion for resolving.
        /// </summary>
        /// <param name="attributeDefinition">The definition to resolve IFNR against</param>
        /// <returns>Data of favourite</returns>
        [MultiReturn("name", "value", "unknown")]
        public Dictionary<string, object> ToData(AttributeDefinition attributeDefinition)
        {
            var ifnrMap = attributeDefinition
                .DefinitionCollection
                .AttributeDefinition
                .ToDictionary(a => a.Ifnr);

            return new Dictionary<string, object>()
            {
                { "name", Favourite?
                        .AttributeSet
                        .Attributes
                        .Where(a => ifnrMap.ContainsKey(a.Ifnr))
                        .Select(a => ifnrMap[a.Ifnr]?.Text.Trim() ?? $"{a.Ifnr}") },
                { "value", Favourite?
                        .AttributeSet
                        .Attributes
                        .Select(a => a.Value) },
                { "unknown", Favourite?
                        .AttributeSet
                        .Attributes
                        .Where(a => !ifnrMap.ContainsKey(a.Ifnr))
                        .Select(a => a.Ifnr) }
            };
        }

        /// <summary>
        /// Dumps the data of favourite.
        /// </summary>
        /// <returns>Data of favourite</returns>
        [MultiReturn("ifnr", "value")]
        public Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
            {
                { "ifnr", Favourite?.AttributeSet.Attributes.Select(a => a.Ifnr) },
                { "value", Favourite?.AttributeSet.Attributes.Select(a => a.Value) }
            };
        }

        /// <summary>
        /// Returns unique attribute names.
        /// </summary>
        /// <param name="attributeDefinition">The attribute defintion</param>
        /// <param name="attributeFavourites">The favourites</param>
        /// <param name="excludeByIfnr">Attribute to be excluded given its <c>Ifnr</c></param>
        /// <returns>An array of names</returns>
        public static string[] ToAttributeNames(AttributeDefinition attributeDefinition, AttributeFavourite[] attributeFavourites, long[] excludeByIfnr)
        {
            if (null == attributeDefinition || null == attributeFavourites)
                return new string[] { };

            long[] blackListIfNr = null != excludeByIfnr ? excludeByIfnr : new long[0];
            Array.Sort(blackListIfNr);

            var ifnrMap = attributeDefinition
                .DefinitionCollection
                .AttributeDefinition
                .Where(a => Array.BinarySearch(blackListIfNr, a.Ifnr) < 0) // Only those which are not in the blacklist
                .ToDictionary(a => a.Ifnr);

            return attributeFavourites
                .SelectMany(af => af.Favourite
                    .AttributeSet
                    .Attributes
                    .Where(a => Array.BinarySearch(blackListIfNr, a.Ifnr) < 0) // Only those which are not in the blacklist
                    .Select(a => ifnrMap[a.Ifnr]?.Text.Trim()))
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Dumps a matrix of attribute names versus values for each favourite.
        /// </summary>
        /// <param name="attributeDefinition">The definition</param>
        /// <param name="attributeFavourites">The favourites</param>
        /// <param name="header">The attibute name header</param>
        /// <returns>A matrix where each rows is a favourite and columns denote the attribute values</returns>
        internal static string[][] ToAttributeValueData(AttributeDefinition attributeDefinition, 
            IEnumerable<AttributeFavourite> attributeFavourites, Tuple<string, int>[] header)
        {
            if (null == attributeDefinition || null == attributeFavourites)
                return new string[][] { };

            List<string[]> rows = new List<string[]>();

            // Generate header
            rows.Add(new string[header.Length + 1]);
            foreach (var e in header)
                rows[0][e.Item2 + 1] = e.Item1;

            foreach (var af in attributeFavourites)
            {
                // First column as favourite name
                string[] row = new string[header.Length + 1];
                row[0] = af.Name;

                // Generate favourite of given level
                var data = af
                    .ToAttributeValue(attributeDefinition)
                    .ToDictionary(a => a.Item1, a => a.Item2);

                // Fill in data
                foreach (var e in header)
                {
                    if (data.ContainsKey(e.Item1))
                        row[e.Item2 + 1] = data[e.Item1];
                    else
                        row[e.Item2 + 1] = "";
                }
                rows.Add(row);
            }

            return rows.ToArray();
        }

        /// <summary>
        /// Dumps a matrix of attribute names versus values for each favourite.
        /// The header reflects unique sorted attribute names.
        /// </summary>
        /// <param name="attributeDefinition">The definition</param>
        /// <param name="attributeFavourites">The favourites</param>
        /// <param name="excludeByIfnr">Attribute to be excluded given its <c>Ifnr</c></param>
        /// <returns>A matrix where each rows is a favourite and columns denote the attribute values</returns>
        public static string[][] ToAttributeValueData(AttributeDefinition attributeDefinition, AttributeFavourite[] attributeFavourites, long[] excludeByIfnr)
        {
            var header = ToAttributeNames(attributeDefinition, attributeFavourites, excludeByIfnr)
                .OrderBy(a => a)
                .Select((a, i) => new Tuple<string, int>(a, i))
                .ToArray();

            return ToAttributeValueData(attributeDefinition, attributeFavourites, header);
        }

        /// <summary>
        /// Generates a custom string representation.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"{Name} ({Favourite.Version}) ({Favourite.AttributeSet.Attributes.Count})";
        }
    }
}
