using System;
using System.Collections.Generic;
using System.Linq;

namespace IlseDynamo.Allplan
{
    /// <summary>
    /// An attribute level matrix associating LOI indexes versus attribute names
    /// accross cumulative level definitions.
    /// </summary>
    public class AttributeMatrix
    {
        #region Internals

        internal SortedDictionary<int, AttributeLevels> AttributeLevels { get; private set; }

        internal AttributeMatrix()
        {
            AttributeLevels = new SortedDictionary<int, AttributeLevels>();
        }

        #endregion

        /// <summary>
        /// Cumulative level creation.
        /// </summary>
        /// <param name="level">The maximum level</param>
        /// <returns>A custom attribute level aggregating all lower levels</returns>
        public AttributeLevels AllUpToLevel(int level)
        {
            return new AttributeLevels
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
        public static AttributeMatrix ByAttributeLevels(AttributeLevels[] attributeLevels)
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
        /// <param name="defaultLevel">Default level (1)</param>
        /// <returns></returns>
        public static AttributeMatrix ByData(object[][] levelAndAttribute, int defaultLevel = 1)
        {
            return ByData(levelAndAttribute
                .Where(row => row?.Length > 1)
                .Where(row => row.All(col => null != col))
                .Select(row => 
                {
                    try 
                    {
                        var level = int.Parse(row[0].ToString());
                        return new Tuple<int, string>(level, row[1].ToString());
                    }
                    catch(Exception e)
                    {
                        return new Tuple<int, string>(defaultLevel, row[1].ToString());
                    }
                }));
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
                .Select(g => new AttributeLevels { Level = g.Key, Attributes = g.ToArray() }))
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
