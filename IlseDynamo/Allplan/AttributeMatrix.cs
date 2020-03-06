using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allplan
{
    public class AttributeMatrix
    {
        #region Internals

        internal SortedDictionary<int, AttributeLevel> AttributeLevels { get; private set; } = new SortedDictionary<int, AttributeLevel>();

        internal AttributeMatrix()
        {
        }

        #endregion

        public AttributeLevel AllUpToLevel(int level)
        {
            return new AttributeLevel
            {
                Level = level,
                Attributes = AttributeLevels.TakeWhile(l => l.Key <= level).SelectMany(l => l.Value.Attributes).Distinct().ToArray()
            };
        }

        public int[] Levels 
        { 
            get => AttributeLevels.Keys.ToArray(); 
        }


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
