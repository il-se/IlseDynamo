using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;

using Autodesk.DesignScript.Runtime;

namespace GGUStratic
{    
    [IsVisibleInDynamoLibrary(false)]
    public class Drilling : DrillingModelElement
    {
        public Position Position { get; set; }

        public SoilLayer[] SoilLayers { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("depth")]
        public double Depth { get; set; }

        [XmlAttribute("document")]
        public string Document { get; set; }

        [XmlAttribute("date")]
        public string Date { get; set; }
    }
}
