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
