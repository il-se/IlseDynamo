using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

namespace Allplan
{
    /// <summary>
    /// Level Of Information structure comprised of numerical level and a set of attribute names which
    /// have to exist
    /// </summary>
    public class AttributeLevel
    {
        #region Internals
                
        internal AttributeLevel()
        {
        }

        #endregion

        /// <summary>
        /// A new LOI by level and attributes.
        /// </summary>
        /// <param name="level">The level</param>
        /// <param name="attributes">The attributes</param>
        /// <returns></returns>
        public static AttributeLevel ByLevelAndAttributes(int level, string[] attributes)
        {
            var set = new HashSet<string>(attributes);
            return new AttributeLevel
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
        /// Gets the attribute value matrix of this attribute level.
        /// </summary>
        /// <param name="attributeDefinition">The definition</param>
        /// <param name="attributeFavourites">The template favourites</param>
        /// <returns>A data matrix of favourite names versus attribute values</returns>
        public string[][] ToAttributeValueData(AttributeDefinition attributeDefinition, AttributeFavourite[] attributeFavourites)
        {
            var header = Attributes.OrderBy(a => a)
                .Select((a, i) => new Tuple<string, int>(a, i))
                .ToArray();
            // Get the level of each favourite according to this level
            return AttributeFavourite.ToAttributeValueData(
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
