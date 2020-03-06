using System.Xml.Serialization;

using Autodesk.DesignScript.Runtime;

#pragma warning disable CS1591 // Missing comments only public methods / accessors

namespace GGUStratic.Data
{
    [IsVisibleInDynamoLibrary(false)]
    public class SoilMainType : DrillingModelElement
    {
        [XmlAttribute("shortname")]
        public string ShortName { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}

#pragma warning restore CS1591 // Missing comments only public methods / accessors