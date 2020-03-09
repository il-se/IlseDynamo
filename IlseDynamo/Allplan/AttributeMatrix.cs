using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allplan
{
    /// <summary>
    /// An attribute level matrix associating LOI indexes versus attribute names
    /// accross cumulative level definitions.
    /// </summary>
    public class AttributeMatrix
    {
        #region Internals

        internal SortedDictionary<int, AttributeLevel> AttributeLevels { get; private set; } = new SortedDictionary<int, AttributeLevel>();

        internal AttributeMatrix()
        {
        }

        #endregion

        /// <summary>
        /// Cumulative level creation.
        /// </summary>
        /// <param name="level">The maximum level</param>
        /// <returns>A custom attribute level aggregating all lower levels</returns>
        public AttributeLevel AllUpToLevel(int level)
        {
            return new AttributeLevel
            {
                Level = level,
                Attributes = AttributeLevels.TakeWhile(l => l.Key <= level).SelectMany(l => l.Value.Attributes).Distinct().ToArray()
            };
        }

        /// <summary>
        /// Returns the LOI indexes of this matrix.
        /// </summary>
        public int[] Levels 
        { 
            get => AttributeLevels.Keys.ToArray(); 
        }

        /// <summary>
        /// A new matrix by given levels.
        /// </summary>
        /// <param name="attributeLevels"></param>
        /// <returns></returns>
        public static AttributeMatrix ByAttributeLevels(AttributeLevel[] attributeLevels)
        {
            var matrix = new AttributeMatrix();
            foreach (var level in attributeLevels)
                matrix.AttributeLevels[level.Level] = level;
            return matrix;
        }

        /// <summary>
        /// Gets the attribute value matrix of a given attribute level.
        /// </summary>
        /// <param name="attributeDefinition">The definition</param>
        /// <param name="attributeLevel">The attribute level</param>
        /// <param name="attributeFavourites">The template favourites</param>
        /// <returns>A data matrix of favourite names versus attribute values</returns>
        public string[][] GetAttributeValueMatrix(AttributeDefinition attributeDefinition, AttributeLevel attributeLevel, AttributeFavourite[] attributeFavourites)
        {
            List<string[]> rows = new List<string[]>();

            var header = attributeLevel.Attributes
                .OrderBy(a => a)
                .Select((a, i) => new Tuple<string, int>(a, i))
                .ToArray();

            // Generate header
            rows.Add(new string[attributeLevel.Attributes.Length + 1]);
            foreach (var e in header)
                rows[0][e.Item2 + 1] = e.Item1;

            foreach (var af in attributeFavourites)
            {
                // First column as favourite name
                string[] row = new string[attributeLevel.Attributes.Length + 1];                
                row[0] = af.Name;

                // Generate favourite of given level
                var levelFavourite = af.OfLevel(attributeLevel, attributeDefinition);
                var data = levelFavourite
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
        /// New LOI definitions by given data import.
        /// </summary>
        /// <param name="levelAndAttribute">Rowwise data with <c>Level index</c> and <c>Attribute name</c></param>
        /// <returns></returns>
        public static AttributeMatrix ByData(object[][] levelAndAttribute)
        {
            return ByData(levelAndAttribute.Select(r => new Tuple<int, string>(int.Parse(r[0].ToString()), r[1].ToString())));
        }

        private static AttributeMatrix ByData(IEnumerable<Tuple<int, string>> levelAndAttribute)
        {
            var matrix = new AttributeMatrix();
            foreach (var level in levelAndAttribute
                .ToLookup(t => t.Item1, t => t.Item2)
                .Select(g => new AttributeLevel { Level = g.Key, Attributes = g.ToArray() }))
            {
                matrix.AttributeLevels[level.Level] = level;
            }
            return matrix;
        }

        public static AttributeMatrix ByData(int[] levels, string[] attributeNames)
        {
            return ByData(Enumerable.Range(0, Math.Min(levels.Length, attributeNames.Length)).Select(i => new Tuple<int, string>(levels[i], attributeNames[i])));
        }
    }
}
