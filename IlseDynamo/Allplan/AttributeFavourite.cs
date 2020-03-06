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
        /// Save this favourite as XML file using existing file name.
        /// </summary>
        public void Save()
        {
            AttributeContainer.WriteTo(FileName);
        }

        /// <summary>
        /// Saves this favourite as new XML file (using atfanfx-extension).
        /// </summary>
        /// <param name="fileName">The file name</param>
        public void SaveAs(string fileName)
        {
            AttributeContainer.WriteTo(fileName);
        }

        /// <summary>
        /// Gets the file name (without extension and path).
        /// </summary>
        /// <returns>The file name</returns>
        public string GetName()
        {
            return Path.GetFileNameWithoutExtension(FileName);
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
                FileName = null,
                AttributeContainer = AllplanAttributesContainer.Create(
                    attributeDefinition.DefinitionCollection.AttributeDefinition.Where(a => nameSet.Contains(a.Text)), "2019")
            };
        }

        public AttributeFavourite OfLevel(AttributeLevel attributeLevel, AttributeDefinition attributeDefinition)
        {
            var nameSet = new HashSet<string>(attributeLevel.Attributes);
            var ifnrSet = attributeDefinition.DefinitionCollection.AttributeDefinition
                .Where(a => nameSet.Contains(a.Text))
                .Select(a => a.Ifnr)
                .ToArray();

            return new AttributeFavourite
            {
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

        /// <summary>
        /// Dumps the data of favourite using the given defintion for resolving.
        /// </summary>
        /// <param name="attributeDefinition">The definition to resolve IFNR against</param>
        /// <returns>Data of favourite</returns>
        [MultiReturn("name", "value")]
        public Dictionary<string, object> ToData(AttributeDefinition attributeDefinition)
        {
            var ifnrMap = attributeDefinition.DefinitionCollection.AttributeDefinition.ToDictionary(a => a.Ifnr);
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

    }
}
