using System.Xml.Serialization;
using Autodesk.DesignScript.Runtime;

#pragma warning disable CS1591 // Missing comments only public methods / accessors

namespace GGUStratic.Data
{
    [IsVisibleInDynamoLibrary(false)]
    public class Position
    {
        [XmlAttribute(AttributeName = "easting")]
        public double Easting;
        [XmlAttribute(AttributeName = "northing")]
        public double Northing;
        [XmlAttribute(AttributeName = "height")]
        public double Height;
    }
}

#pragma warning restore CS1591 // Missing comments only public methods / accessors
