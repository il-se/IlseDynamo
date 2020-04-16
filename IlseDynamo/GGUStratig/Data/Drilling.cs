using System.Xml;
using System.Xml.Serialization;

using Autodesk.DesignScript.Runtime;

#pragma warning disable CS1591 // Missing comments only public methods / accessors

namespace GGUStratic.Data
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

#pragma warning restore CS1591 // Missing comments only public methods / accessors