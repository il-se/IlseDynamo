using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using IlseDynamo.Internal;
using IlseDynamo.Data.Allplan;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes.CustomNodes;
using System.Text.RegularExpressions;

namespace IlseDynamo.Allplan
{
    /// <summary>
    /// Node mapping for an Allplan Attribite Favourite (*.atfanfx).
    /// </summary>
    public class AttributeFavourites : LocalResourceFile
    {
        #region Internal

        internal const string EXTENSION = ".atfanfx";

        internal AllplanAttributesContainer Favourite { get; set; }

        internal AttributeFavourites()
        {
        }

        #endregion

        /// <inheritdoc/>
        public new string Folder { get => base.Folder; }
        /// <inheritdoc/>
        public new string Name { get => base.Name; }
        /// <inheritdoc/>
        public new string BaseFolder { get => base.BaseFolder; }

        /// <summary>
        /// Imports an Allplan Favourite file.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <param name="basePath">If given, a save will keep folder hierarchy the same as input hierarchy</param>
        /// <returns>A favourite</returns>
        public static AttributeFavourites ByFileName(string fileName, string basePath = null)
        {
            return new AttributeFavourites
            {
                FileName = fileName,
                BasePath = basePath,
                Favourite = AllplanAttributesContainer.ReadFrom(fileName)
            };
        }

        /// <summary>
        /// Create a new favourite by names from given attribute definition.
        /// </summary>
        /// <param name="favouriteName">Favourite name</param>
        /// <param name="favouriteBasePath">Favourite base path</param>
        /// <param name="attributeNames">The names</param>
        /// <param name="attributes">The attribute definition</param>
        /// <param name="caseInsensitive">Case insensitive (false by default)</param>
        /// <returns>The favourite from matches and missing names by definition</returns>
        [MultiReturn("favourite", "missing")]
        public static Dictionary<string, object> ByAttributeNames(string favouriteName, string favouriteBasePath, 
            string[] attributeNames, Attributes attributes, bool caseInsensitive = false)
        {
            var matches = attributeNames.Select(name =>
                (name, attributes.AttributeCollection.AttributeDefinition.Find(a => 0 == string.Compare(a.Name, name, caseInsensitive))))
                .ToArray();

            return new Dictionary<string, object>()
            {
                { "favourite", new AttributeFavourites
                    { 
                        FileName = Path.Combine(favouriteBasePath, favouriteName + EXTENSION),                       
                        BasePath = favouriteBasePath,
                        Favourite = new AllplanAttributesContainer
                        {
                            AttributeSet = new AllplanAttributeSet
                            {
                                Attributes = matches
                                    .Where(m => m.Item2 != null)
                                    .Select(m => new AllplanAttribute{ Ifnr = m.Item2.Ifnr, Suffix = m.Item2.Datatype })
                                    .ToList()
                            }
                        }
                    } 
                },
                { "missing", matches.Where(m => m.Item2 == null).Select(m => m.name).ToArray() }
            };
        }

        /// <summary>
        /// Create a new favourite by GUIDs from given attribute definition.
        /// </summary>
        /// <param name="favouriteName">Favourite name</param>
        /// <param name="favouriteBasePath">Favourite base path</param>
        /// <param name="attributeGuids">The GUIDs</param>
        /// <param name="attributes">The attribute definition</param>
        /// <returns>The favourite from matches and missing GUIDs by definition</returns>
        [MultiReturn("favourite", "missing")]
        public static Dictionary<string, object> ByAttributeGUIDs(string favouriteName, string favouriteBasePath, 
            Guid[] attributeGuids, Attributes attributes)
        {
            var matches = attributeGuids.Select(guid =>
                (guid, attributes.AttributeCollection.AttributeDefinition.Find(a => a.DefinitionId == guid)))
                .ToArray();

            return new Dictionary<string, object>()
            {
                { "favourite", new AttributeFavourites
                    {
                        FileName = Path.Combine(favouriteBasePath, favouriteName + EXTENSION),
                        BasePath = favouriteBasePath,
                        Favourite = new AllplanAttributesContainer
                        {
                            AttributeSet = new AllplanAttributeSet
                            {
                                Attributes = matches
                                    .Where(m => m.Item2 != null)
                                    .Select(m => new AllplanAttribute{ Ifnr = m.Item2.Ifnr, Suffix = m.Item2.Datatype })
                                    .ToList()
                            }
                        }
                    }
                },
                { "missing", matches.Where(m => m.Item2 == null).Select(m => m.guid).ToArray() }
            };
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

        public static AttributeFavourites RenameBy(AttributeFavourites favourites, string pattern, string replaceBy)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline);
            favourites.SetName(regex.Replace(favourites.Name, replaceBy));
            return favourites;
        }

        /// <summary>
        /// Creates a new favourite excluding given attributes.
        /// </summary>
        /// <param name="names">The attribute names.</param>
        /// <param name="attributeDefinition">The definition to resolve IFNR against</param>
        /// <returns>A new favourite</returns>
        public AttributeFavourites Exclude(string[] names, Attributes attributeDefinition)
        {
            var nameSet = new HashSet<string>(names);
            var ifNrSet = attributeDefinition.AttributeCollection
                .AttributeDefinition
                .Where(a => nameSet.Contains(a.Text.Trim()))
                .Select(a => a.Ifnr)
                .ToArray();

            return new AttributeFavourites
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
        public static AttributeFavourites ByLevel(AttributeLevels attributeLevel, Attributes attributeDefinition)
        {
            var nameSet = attributeLevel.Attributes.ToSet(false);
            return new AttributeFavourites
            {
                FileName = $"{attributeLevel.Level}",
                Favourite = AllplanAttributesContainer.Create(
                    attributeDefinition.AttributeCollection
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
        public AttributeFavourites OfLevel(AttributeLevels attributeLevel, Attributes attributeDefinition)
        {
            var nameSet = attributeLevel.Attributes.ToSet(false);
            var ifnrSet = attributeDefinition.AttributeCollection.AttributeDefinition
                .Where(a => nameSet.Contains(a.Text.Trim()))
                .Select(a => a.Ifnr)
                .ToArray();

            return new AttributeFavourites
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
        public AttributeFavourites Exclude(long[] ifnr)
        {
            return new AttributeFavourites
            {
                FileName = FileName,
                BasePath = BasePath,
                Favourite = Favourite?.ExcludeByIfNr(ifnr)
            };
        }

        internal IEnumerable<Tuple<string, string>> ToAttributeValue(Attributes attributeDefinition)
        {
            var ifnrMap = attributeDefinition
                .AttributeCollection
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
        public Dictionary<string, object> ToData(Attributes attributeDefinition)
        {
            var ifnrMap = attributeDefinition
                .AttributeCollection
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
        public static string[] ToAttributeNames(Attributes attributeDefinition, AttributeFavourites[] attributeFavourites, long[] excludeByIfnr)
        {
            if (null == attributeDefinition || null == attributeFavourites)
                return new string[] { };

            long[] blackListIfNr = null != excludeByIfnr ? excludeByIfnr : new long[0];
            Array.Sort(blackListIfNr);

            var ifnrMap = attributeDefinition
                .AttributeCollection
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
        internal static string[][] ToAttributeValueData(Attributes attributeDefinition, 
            IEnumerable<AttributeFavourites> attributeFavourites, Tuple<string, int>[] header)
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
        public static string[][] ToAttributeValueData(Attributes attributeDefinition, AttributeFavourites[] attributeFavourites, long[] excludeByIfnr)
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
