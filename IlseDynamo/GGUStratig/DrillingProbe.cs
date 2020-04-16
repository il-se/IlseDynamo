using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Autodesk.DesignScript.Geometry;

using GGUStratic.Data;

namespace GGUStratig
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
        /// <returns>The 3D position</returns>
        public Point GetPosition()
        {
            return Point.ByCoordinates(Probe.Position.Easting, Probe.Position.Northing, Probe.Position.Height);
        }

        /// <summary>
        /// The name of probe.
        /// </summary>
        /// <returns>The name of the probe</returns>
        public string GetName()
        {
            return Probe.Name;
        }

        /// <summary>
        /// Gets the associated labels of each layer
        /// </summary>
        /// <returns>An info per layer</returns>
        public string[] GetLayerInfo()
        {
            return Probe?.SoilLayers.Select(l => l.Info).ToArray();
        }

        /// <summary>
        /// Gets the associated end depth for each layer
        /// </summary>
        /// <returns>A depth per layer</returns>
        public double[] GetLayerDepth()
        {
            return Probe?.SoilLayers.Select(l => l.Depth).ToArray();
        }

        /// <summary>
        /// Gets the associated short type classifiers for each layer.
        /// </summary>
        /// <returns>List of type classifiers per layer</returns>
        public string[][] GetLayerTypes()
        {
            return Probe?.SoilLayers.Select(l => l.SoilMainTypes.Select(t => t.ShortName).ToArray()).ToArray();
        }

        /// <summary>
        /// Gets an string aggregation of type classifiers per layer.
        /// </summary>
        /// <param name="separator">The classifier separator (by default ',')</param>
        /// <returns>Aggregated type infos per layer</returns>
        public string[] GetLayerTypeInfo(string separator = ",")
        {
            return GetLayerTypes().Select(l => string.Join(separator, l)).ToArray();
        }

        /// <summary>
        /// Gets the associated layer type names for each layer
        /// </summary>
        /// <returns>A list of type names per layer</returns>
        public string[][] GetLayerTypeNames()
        {
            return Probe?.SoilLayers.Select(l => l.SoilMainTypes.Select(t => t.Name).ToArray()).ToArray();
        }

        /// <summary>
        /// Gets an string aggregation of type names per layer.
        /// </summary>
        /// <param name="separator">The type name separator (by default ',')</param>
        /// <returns>Aggregated type name infos per layer</returns>
        public string[] GetLayerTypeNameInfo(string separator = ",")
        {
            return GetLayerTypeNames().Select(l => string.Join(separator, l)).ToArray();
        }

        /// <summary>
        /// Gets a representative data matrix of the probe (name, position, representative layer data).
        /// </summary>
        /// <returns>Representative layer data per layer</returns>
        public object[] GetTabularData()
        {
            return Data.SelectMany(data => data(Probe)).ToArray();
        }
    }
}
