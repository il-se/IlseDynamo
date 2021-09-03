using System;
using System.Collections.Generic;
using System.Linq;

using Internal;

using Autodesk.DesignScript.Runtime;

namespace IlseDynamo.Allplan
{
    /// <summary>
    /// Level Of Information structure comprised of numerical level and a set of attribute names which
    /// have to exist
    /// </summary>
    public class AttributeLevels
    {
        #region Internals
                
        internal AttributeLevels()
        {
        }

        #endregion

        /// <summary>
        /// A new LOI by level and attributes.
        /// </summary>
        /// <param name="level">The level</param>
        /// <param name="attributes">The attributes</param>
        /// <param name="ignoreCase">Whether being case sensitive or not. Default is false</param>
        /// <returns>A new attribute level</returns>
        public static AttributeLevels ByLevelAndAttributes(int level, string[] attributes, bool ignoreCase = false)
        {
            var set = attributes.ToSet(ignoreCase);
            return new AttributeLevels
            {
                Level = level,
                Attributes = set.ToArray()
            };
        }

        /// <summary>
        /// The level of this LOI.
        /// </summary>
        public int Level { get; internal set; } = 0;

        /// <summary>
        /// The associated attribute names of this LOI.
        /// </summary>
        public string[] Attributes { get; set; } = new string[] { };

        /// <summary>
        /// Merges the given attribute names into the attribute level.
        /// </summary>
        /// <param name="attributes">An array of attribute names</param>
        /// <param name="ignoreCase">Whether being case sensitive or not. Default is true</param>
        /// <returns>A modified level</returns>
        [MultiReturn("attributeLevel", "added")]
        public Dictionary<string, object> Merge(string[] attributes, bool ignoreCase = true)
        {
            var set = Attributes.ToSet(ignoreCase);
            var added = new List<string>();
            foreach (var attribute in attributes)
            {
                if (set.Add(attribute))
                    added.Add(attribute);
            }

            Attributes = set.ToArray();
            return new Dictionary<string, object>()
            {
                { "attributeLevel", this },
                { "added", added.ToArray() }
            };
        }

        /// <summary>
        /// Gets the attribute value matrix of this attribute level.
        /// </summary>
        /// <param name="attributeDefinition">The definition</param>
        /// <param name="attributeFavourites">The template favourites</param>
        /// <returns>A data matrix of favourite names versus attribute values</returns>
        public string[][] ToAttributeValueData(Attributes attributeDefinition, AttributeFavourites[] attributeFavourites)
        {
            var header = Attributes.OrderBy(a => a)
                .Select((a, i) => new Tuple<string, int>(a, i))
                .ToArray();
            // Get the level of each favourite according to this level
            return AttributeFavourites.ToAttributeValueData(
                    attributeDefinition,                 
                    attributeFavourites.Select(f => f.OfLevel(this, attributeDefinition)), 
                    header);
        }

        /// <summary>
        /// Generates a custom string representation.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"AttributeLevel{GetHashCode()}: {Level} - Count = {Attributes.Length}";
        }
    }
}
