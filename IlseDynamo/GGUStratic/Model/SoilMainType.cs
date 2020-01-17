using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

using Autodesk.DesignScript.Runtime;

namespace GGUStratic
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