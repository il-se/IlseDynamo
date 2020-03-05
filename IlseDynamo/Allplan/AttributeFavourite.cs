using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Allplan.Data;

using Autodesk.DesignScript.Runtime;

namespace Allplan
{
    /// <summary>
    /// Node mapping for an Allplan Attribite Favourite.
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

        public void Save()
        {
            AttributeContainer?.WriteTo(FileName);
        }

        public void SaveAs(string fileName)
        {
            AttributeContainer?.WriteTo(fileName);
        }

        public string GetName()
        {
            return Path.GetFileNameWithoutExtension(FileName);
        }

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

        public AttributeFavourite Exclude(long[] ifnr)
        {
            return new AttributeFavourite
            {
                AttributeContainer = AttributeContainer?.ExcludeByIfNr(ifnr)
            };
        }

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
