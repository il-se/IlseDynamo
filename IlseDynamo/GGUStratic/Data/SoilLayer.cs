using System;
using System.Collections.Generic;

using System.Xml.Serialization;

using Autodesk.DesignScript.Runtime;

#pragma warning disable CS1591 // Missing comments only public methods / accessors

namespace GGUStratic.Data
{
    [IsVisibleInDynamoLibrary(false)]
    public class SoilLayer : DrillingModelElement
    {
        // Serialized list of GUIDs only
        [XmlAttribute("mainTypes")]
        public string MainTypesGuids { get; set; } = "";

        [XmlAttribute("depth")]
        public double Depth { get; set; }

        [XmlAttribute("info")]
        public string Info { get; set; }

        public SoilLayer() : base()
        {
            // Register access to main types
            Register(nameof(MainTypesGuids), (r) => r.SoilMainTypes);
        }

        public IEnumerable<SoilMainType> SoilMainTypes { get => base.Get<SoilMainType>(nameof(MainTypesGuids)); }
    }
}

#pragma warning restore CS1591 // Missing comments only public methods / accessors
