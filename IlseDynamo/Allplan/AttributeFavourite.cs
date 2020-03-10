using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
        private AllplanAttributesContainer AttributeContainer { get; set; }

        internal AttributeFavourite()
        {
        }

        #endregion

        /// <summary>
        /// Imports an Allplan Favourite file.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>A favourite</returns>
        public static AttributeFavourite ByFileName(string fileName)
        {
            return new AttributeFavourite
            {
                FileName = fileName,
                AttributeContainer = AllplanAttributesContainer.ReadFrom(fileName)
            };
        }

        /// <summary>
        /// Saves this favourite as new XML file (using atfanfx-extension).
        /// </summary>
        /// <param name="pathName">The file name</param>
        /// <returns>The written file name</returns>
        public string SaveAs(string pathName)
        {
            var trimmedPath = Path.GetDirectoryName($"{pathName}{Path.DirectorySeparatorChar}");
            var finalFilePath = $"{trimmedPath}{Path.DirectorySeparatorChar}{Name}{EXTENSION}";
            AttributeContainer.WriteTo(finalFilePath);
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
                .Where(a => nameSet.Contains(a.Text))
                .Select(a => a.Ifnr)
                .ToArray();

            return new AttributeFavourite
            {
                AttributeContainer = AttributeContainer?.ExcludeByIfNr(ifNrSet)
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
            var nameSet = new HashSet<string>(attributeLevel.Attributes);
            return new AttributeFavourite
            {
                FileName = $"{attributeLevel.Level}",
                AttributeContainer = AllplanAttributesContainer.Create(
                    attributeDefinition.DefinitionCollection.AttributeDefinition.Where(a => nameSet.Contains(a.Text)), "2019")
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
            var nameSet = new HashSet<string>(attributeLevel.Attributes);
            var ifnrSet = attributeDefinition.DefinitionCollection.AttributeDefinition
                .Where(a => nameSet.Contains(a.Text))
                .Select(a => a.Ifnr)
                .ToArray();

            return new AttributeFavourite
            {
                FileName = $"{Path.GetDirectoryName(FileName)}{Path.DirectorySeparatorChar}{Name}_{attributeLevel.Level}{EXTENSION}",
                AttributeContainer = AttributeContainer.FilterByIfNr(ifnrSet)
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
                AttributeContainer = AttributeContainer?.ExcludeByIfNr(ifnr)
            };
        }

        internal IEnumerable<Tuple<string, string>> ToAttributeValue(AttributeDefinition attributeDefinition)
        {
            var ifnrMap = attributeDefinition
                .DefinitionCollection
                .AttributeDefinition
                .ToDictionary(a => a.Ifnr);

            return AttributeContainer?.AttributeSet
                .Attributes
                .Select(a => new Tuple<string, string>(ifnrMap[a.IfNr]?.Text, a.Value));
        }

        /// <summary>
        /// Dumps the data of favourite using the given defintion for resolving.
        /// </summary>
        /// <param name="attributeDefinition">The definition to resolve IFNR against</param>
        /// <returns>Data of favourite</returns>
        [MultiReturn("name", "value")]
        public Dictionary<string, object> ToData(AttributeDefinition attributeDefinition)
        {
            var ifnrMap = attributeDefinition
                .DefinitionCollection
                .AttributeDefinition
                .ToDictionary(a => a.Ifnr);

            return new Dictionary<string, object>()
            {
                { "name", AttributeContainer?.AttributeSet.Attributes.Select(a => ifnrMap[a.IfNr]?.Text) },
                { "value", AttributeContainer?.AttributeSet.Attributes.Select(a => a.Value) }
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
                { "ifnr", AttributeContainer?.AttributeSet.Attributes.Select(a => a.IfNr) },
                { "value", AttributeContainer?.AttributeSet.Attributes.Select(a => a.Value) }
            };
        }

        /// <summary>
        /// Returns unique attribute names.
        /// </summary>
        /// <param name="attributeDefinition">The attribute defintion</param>
        /// <param name="attributeFavourites">The favourites</param>
        /// <returns></returns>
        public static string[] ToAttributeNames(AttributeDefinition attributeDefinition, AttributeFavourite[] attributeFavourites)
        {
            var ifnrMap = attributeDefinition
                .DefinitionCollection
                .AttributeDefinition
                .ToDictionary(a => a.Ifnr);

            return attributeFavourites
                .SelectMany(af => af.AttributeContainer.AttributeSet.Attributes.Select(a => ifnrMap[a.IfNr]?.Text))
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
        /// <returns>A matrix where each rows is a favourite and columns denote the attribute values</returns>
        public static string[][] ToAttributeValueData(AttributeDefinition attributeDefinition, AttributeFavourite[] attributeFavourites)
        {
            var header = ToAttributeNames(attributeDefinition, attributeFavourites)
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
            return $"{Name} ({AttributeContainer.Version}) ({AttributeContainer.AttributeSet.Attributes.Count})";
        }
    }
}
