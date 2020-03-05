using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using ProtoCore.DSASM;

namespace Allplan
{
    /// <summary>
    /// Level Of Information structure comprised of numerical level and a set of attribute names which
    /// have to exist
    /// </summary>
    public class LOI
    {
        #region Internals

        internal int Level { get; private set; }
        internal string[] Attributes { get; private set; } = new string[] { };

        internal LOI()
        {
        }

        #endregion

        /// <summary>
        /// A new LOI by level and attributes.
        /// </summary>
        /// <param name="level">The level</param>
        /// <param name="attributes">The attributes</param>
        /// <returns></returns>
        public static LOI ByLevelAttributes(int level, string[] attributes)
        {
            var set = new HashSet<string>(attributes);
            return new LOI
            {
                Level = level,
                Attributes = set.ToArray()
            };
        }

        /// <summary>
        /// New LOI definitions by given data import.
        /// </summary>
        /// <param name="levelAndAttribute">Rowwise data with <c>Level index</c> and <c>Attribute name</c></param>
        /// <returns></returns>
        public static LOI[] ByData(object[][] levelAndAttribute)
        {
            List<LOI> loiList = new List<LOI>();
            var levelLookUp = levelAndAttribute.Select(r =>
            {
                return new Tuple<int, string>(int.Parse(r[0].ToString()), r[1].ToString());
            }).ToLookup(t => t.Item1, t => t.Item2);

            return levelLookUp.Select(g => new LOI { Level = g.Key, Attributes = g.ToArray() }).ToArray();
        }

        public int GetLevel()
        {
            return Level;
        }
    }
}
