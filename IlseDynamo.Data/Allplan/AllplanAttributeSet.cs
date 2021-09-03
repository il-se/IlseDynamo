using System;
using System.Xml;

using System.Collections.Generic;

namespace IlseDynamo.Data.Allplan
{
    public class AllplanAttributeSet
    {
        internal const string ELEMENT_NAME = "NEM_ATTRIB_SET";

        public string Key { get; set; } = "0 0 0 0 0 0 0 0";

        public IList<AllplanAttribute> Attributes { get; set; } = new List<AllplanAttribute>();

        internal XmlWriter WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement(ELEMENT_NAME);
            writer.WriteAttributeString("KEY", Key);
            foreach (var attrib in Attributes)
                attrib.WriteTo(writer);
            writer.WriteEndElement();
            return writer;
        }

        internal static AllplanAttributeSet ReadFrom(XmlReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element)
                throw new NotSupportedException($"Expecting element type");
            if (!reader.Name.Equals(ELEMENT_NAME))
                throw new NotSupportedException($"Expecting '{ELEMENT_NAME}' as prefix. Got '{reader.Name}'");

            var attribSet = new AllplanAttributeSet
            {
                Key = reader.GetAttribute("KEY")
            };
            
            while(reader.Read())
            {
                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.StartsWith(AllplanAttribute.ELEMENT_NAME))
                            attribSet.Attributes.Add(AllplanAttribute.ReadFrom(reader));
                        else
                            throw new NotImplementedException($"Missing implementation for element '{reader.Name}'");
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name.Equals(ELEMENT_NAME))
                            return attribSet;
                        break;
                }
            }

            throw new NotSupportedException($"Unexpected end of file");
        }
    }
}