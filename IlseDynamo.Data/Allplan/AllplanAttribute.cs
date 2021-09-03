using System;
using System.Xml;

using System.Collections.Generic;

namespace IlseDynamo.Data.Allplan
{
    public class AllplanAttribute : IEquatable<AllplanAttribute>
    {
        internal const string ELEMENT_NAME = "NEM_ATTRIB";

        public long Ifnr { get; set; }
        public string Value { get; set; }
        public string Suffix { get; set; }

        public override bool Equals(object obj)
        {
            return obj is AllplanAttribute attrib &&
                   Ifnr == attrib.Ifnr &&
                   Value == attrib.Value &&
                   Suffix == attrib.Suffix;
        }

        public bool Equals(AllplanAttribute other)
        {
            return Equals((object)other);
        }

        public override int GetHashCode()
        {
            var hashCode = 1103233092;
            hashCode = hashCode * -1521134295 + Ifnr.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Suffix);
            return hashCode;
        }

        internal XmlWriter WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement($"{ELEMENT_NAME}_{Suffix}");
            writer.WriteElementString("IFNR", $"{Ifnr}");
            writer.WriteElementString("VALUE", Value);
            writer.WriteEndElement();
            return writer;
        }

        internal static AllplanAttribute ReadFrom(XmlReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element)
                throw new NotSupportedException($"Expecting element type");
            if (!reader.Name.StartsWith(ELEMENT_NAME))
                throw new NotSupportedException($"Expecting '{ELEMENT_NAME}' as prefix. Got '{reader.Name}'");

            var attrib = new AllplanAttribute
            {
                Suffix = reader.Name.Replace($"{ELEMENT_NAME}_", "")
            };
            while(reader.Read())
            {
                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.Equals("IFNR"))
                            attrib.Ifnr = long.Parse(reader.ReadElementContentAsString());
                        else if (reader.Name.Equals("VALUE"))
                            attrib.Value = reader.ReadElementContentAsString();
                        else
                            throw new NotImplementedException($"Missing handler for element '{reader.Name}'");
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name.StartsWith(ELEMENT_NAME))
                            return attrib;
                        break;
                }
            }
            throw new NotSupportedException($"Unexpected end of file");            
        }
    }
}