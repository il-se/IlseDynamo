using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Geometry;

namespace GGUStratic
{
    /// <summary>
    /// The descriptive data of a drilling record.
    /// </summary>
    public class DrillingProbe
    {
        #region Internals

        internal Drilling Probe { get; set; }

        internal static NumberFormatInfo Formatter { get; set; } = new NumberFormatInfo 
        { 
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = ""
        };

        internal Func<Drilling, IEnumerable<string>>[] DataConverter = new Func<Drilling, IEnumerable<string>>[]
        {
            (d) => new string[] { d.Name },
            (d) => new string[] { d.Position.Easting.ToString(Formatter) },
            (d) => new string[] { d.Position.Northing.ToString(Formatter) },
            (d) => new string[] { d.Position.Height.ToString(Formatter) },
            (d) => d.SoilLayers.Select(l => new string[] { l.Info, l.Depth.ToString(Formatter) } ).SelectMany(r => r)
        };

        internal Func<Drilling, IEnumerable<object>>[] Data = new Func<Drilling, IEnumerable<object>>[]
{
            (d) => new object[] { d.Name },
            (d) => new object[] { d.Position.Easting },
            (d) => new object[] { d.Position.Northing },
            (d) => new object[] { d.Position.Height },
            (d) => d.SoilLayers.Select(l => new object[] { l.Info, l.Depth } ).SelectMany(r => r)
        };

        internal DrillingProbe()
        {
        }

        #endregion

        /// <summary>
        /// The 3D position of probe.
        /// </summary>
        /// <returns></returns>
        public Point GetPosition()
        {
            return Point.ByCoordinates(Probe.Position.Easting, Probe.Position.Northing, Probe.Position.Height);
        }

        /// <summary>
        /// The name of probe.
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return Probe.Name;
        }

        /// <summary>
        /// Gets the associated labels of each layer
        /// </summary>
        /// <returns></returns>
        public string[] GetLayerInfo()
        {
            return Probe?.SoilLayers.Select(l => l.Info).ToArray();
        }

        /// <summary>
        /// Gets the associated end depth for each layer
        /// </summary>
        /// <returns></returns>
        public double[] GetLayerDepth()
        {
            return Probe?.SoilLayers.Select(l => l.Depth).ToArray();
        }

        /// <summary>
        /// Gets the associated short types for each layer
        /// </summary>
        /// <returns></returns>
        public string[][] GetLayerShortTypes()
        {
            return Probe?.SoilLayers.Select(l => l.SoilMainTypes.Select(t => t.ShortName).ToArray()).ToArray();
        }

        /// <summary>
        /// Gets the associated layer type names for each layer
        /// </summary>
        /// <returns></returns>
        public string[][] GetLayerTypeNames()
        {
            return Probe?.SoilLayers.Select(l => l.SoilMainTypes.Select(t => t.Name).ToArray()).ToArray();
        }

        public object[] GetData()
        {
            return Data.SelectMany(data => data(Probe)).ToArray();
        }
    }
}
