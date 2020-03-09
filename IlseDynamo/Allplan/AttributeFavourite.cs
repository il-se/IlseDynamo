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
            var trimmedPath = Path.GetDirectoryName(pathName);
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
        public static string[] GetUniqueAttributeNames(AttributeDefinition attributeDefinition, AttributeFavourite[] attributeFavourites)
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

        public static string[][] GetAttributeValueMatrix(AttributeDefinition attributeDefinition)
        {
            return null;
        }
    }
}
