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
        /// New LOI definitions by given data import.
        /// </summary>
        /// <param name="levelAndAttribute">Rowwise data with <c>Level index</c> and <c>Attribute name</c></param>
        /// <returns></returns>
        public static AttributeMatrix ByData(object[][] levelAndAttribute)
        {
            return ByData(levelAndAttribute
                .Where(r => r?.Length > 0)
                .Select(r => new Tuple<int, string>(int.Parse(r[0].ToString()), r[1].ToString())));
        }

        /// <summary>
        /// Imports an attribute matrix by given data matrix.
        /// </summary>
        /// <param name="levelAndAttribute">Rowwise data</param>
        /// <returns>A new matrix</returns>
        private static AttributeMatrix ByData(IEnumerable<Tuple<int, string>> levelAndAttribute)
        {
            var matrix = new AttributeMatrix();
            foreach (var level in levelAndAttribute
                .ToLookup(t => t.Item1, t => t.Item2.Trim())
                .Select(g => new AttributeLevel { Level = g.Key, Attributes = g.ToArray() }))
            {
                matrix.AttributeLevels[level.Level] = level;
            }
            return matrix;
        }

        /// <summary>
        /// Imports an attribute matrix by given data matrix.
        /// </summary>
        /// <param name="levels">The level data</param>
        /// <param name="attributeNames">The attribute per level data</param>
        /// <returns>A new matrix</returns>
        public static AttributeMatrix ByData(int[] levels, string[] attributeNames)
        {
            return ByData(
                Enumerable.Range(0, 
                    Math.Min(levels?.Length ?? 0, attributeNames?.Length ?? 0)).Select(i => new Tuple<int, string>(levels[i], attributeNames[i])));
        }
    }
}
