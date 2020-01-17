using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

using Autodesk.DesignScript.Runtime;

namespace GGUStratic
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
