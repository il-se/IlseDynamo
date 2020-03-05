using System;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Globalization;

namespace GGUStratic
{
    [TestClass]
    public class GGUDrillingProbesTests
    {
        [DeploymentItem(@"Resources\GGUDrillingProfilesExample.xml")]
        [TestMethod]
        public void TestReadAndReferencing()
        {
            Assert.IsTrue(File.Exists("GGUDrillingProfilesExample.xml"));
            var profiles = DrillingProfiles.ReadFrom("GGUDrillingProfilesExample.xml");
            Assert.IsNotNull(profiles);

            // Check referencing works
            Assert.IsTrue(profiles.Drillings.Any());
            Assert.IsTrue(profiles.Drillings.Any(d => d.SoilLayers.Any()));
            Assert.IsTrue(profiles.Drillings.Any(d => d.SoilLayers.SelectMany(l => l.SoilMainTypes).Any()));
        }

        [DeploymentItem(@"Resources\GGUDrillingProfilesExample.xml")]
        [TestMethod]
        public void TestDrillingWiseGrouping()
        {
            using (var text = File.CreateText("GGUDrillingProfilesExample.txt"))
            {
                Assert.IsTrue(File.Exists("GGUDrillingProfilesExample.xml"));
                var profiles = DrillingProfiles.ReadFrom(@"GGUDrillingProfilesExample.xml");
                Assert.IsNotNull(profiles);

                foreach (var d in profiles.Drillings)
                {
                    text.Write($"{d.Name}; {d.Position.Easting}; {d.Position.Northing}; {d.Position.Height.ToString("0.00", CultureInfo.InvariantCulture)}; ");
                    string[] soils = d.SoilLayers.Select(l => $"{l.Info}; {l.Depth.ToString("0.00", CultureInfo.InvariantCulture)}").ToArray();
                    for (int i = 1; i < soils.Length; i++)
                        text.Write($"{soils[i]}; ");
                    text.Write("\n");
                }

                text.Close();
            }
        }

    }
}
