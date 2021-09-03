using System;
using System.Linq;

using Autodesk.DesignScript.Geometry;

using IlseDynamo.Data.GGU;

namespace IlseDynamo.GGUStratig
{
    /// <summary>
    /// Wrap GGU stratic soil layer / probes export files.
    /// </summary>
    public class DrillingProbes
    {
        #region Internals

        internal DrillingProfiles ProfileData { get; set; }

        internal DrillingProbes()
        {
        }

        #endregion

        /// <summary>
        /// Reads a GGUStratic XML profile data file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>Profile data</returns>
        public static DrillingProbes ByFileName(string fileName)
        {
            return new DrillingProbes
            {
                ProfileData = DrillingProfiles.ReadFrom(fileName)
            };
        }

        /// <summary>
        /// Get the positions of all drillings.
        /// </summary>
        /// <param name="p0">The base reference point</param>
        /// <returns>An array of depths</returns>
        public Point[] GetPositions(Point p0)
        {
            return ProfileData?.Drillings
                .Select(d => Point.ByCoordinates(d.Position.Easting - p0.X, d.Position.Northing - p0.Y, d.Position.Height - p0.Z))
                .ToArray();
        }

        /// <summary>
        /// Get the positions of all drillings.
        /// </summary>
        /// <param name="uv0">The base reference point</param>
        /// <returns>An array of depths</returns>
        public UV[] GetPositions2D(UV uv0)
        {
            return ProfileData?.Drillings
                .Select(d => UV.ByCoordinates(d.Position.Easting - uv0.U, d.Position.Northing - uv0.V))
                .ToArray();
        }

        /// <summary>
        /// Gets the drilling probe data.
        /// </summary>
        /// <returns></returns>
        public DrillingProbe[] GetProbeData()
        {
            return ProfileData?.Drillings.Select(d => new DrillingProbe { Probe = d }).ToArray();
        }

        /// <summary>
        /// Maximum soil layer count.
        /// </summary>
        /// <returns>Count of soil layers at maximum</returns>
        public int GetMaxLayerCount()
        {
            return ProfileData?.Drillings.Select(d => d.SoilLayers.Length).Max() ?? 0;
        }

        /// <summary>
        /// Maximum soil layer depth.
        /// </summary>
        /// <returns>Depth of drillings at maximum</returns>
        public double GetMaxLayerDepth()
        {
            return ProfileData?.Drillings.SelectMany(d => d.SoilLayers.Select(l => l.Depth)).Max() ?? 0;
        }

        /// <summary>
        /// Names of drilling probes.
        /// </summary>
        /// <returns></returns>
        public string[] GetNames()
        {
            return ProfileData?.Drillings.Select(d => d.Name).ToArray();
        }
    }
}
