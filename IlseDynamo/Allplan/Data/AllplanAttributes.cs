﻿using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;

using System.Collections.Generic;

using Autodesk.DesignScript.Runtime;

#pragma warning disable CS1591 // Missing comments only public methods / accessors

namespace Allplan.Data
{
    [IsVisibleInDynamoLibrary(false)]
    public class AllplanAttributesContainer
    {
        internal const string ELEMENT_NAME = "NEM_ATTRIBUTES";

        public string Version { get; set; }
        public string FileType { get; set; }

        public AllplanAttributeSet AttributeSet { get; set; }

        public void WriteTo(string fileName)
        {
            using(var writer = XmlWriter.Create(
                File.Create(fileName), new XmlWriterSettings 
                { 
                    CloseOutput = true, 
                    Indent = true,
                    NewLineHandling = NewLineHandling.Replace,
                    Encoding = new UnicodeEncoding()
                }))
            {
                WriteTo(writer);
                writer.Close();
            }
        }

        private void Update55()
        {
            var a55 = AttributeSet.Attributes.FirstOrDefault(a => a.Ifnr == 55);
            if (null == a55)
                AttributeSet.Attributes.Add( a55 = new AllplanAttribute { Ifnr = 55, Suffix = "Y" } );

            a55.Value = string.Join(" ", AttributeSet.Attributes.Select(a => a.Ifnr).Where(ifnr => 55 != ifnr));
        }

        internal XmlWriter WriteTo(XmlWriter writer)
        {
            Update55();

            writer.WriteStartElement(ELEMENT_NAME);
            writer.WriteAttributeString("VERSION", Version);
            writer.WriteAttributeString("FILETYPE", FileType);

            AttributeSet.WriteTo(writer);
            writer.WriteEndElement();
            return writer;
        }

        public static AllplanAttributesContainer ReadFrom(string fileName)
        {
            using (var reader = XmlReader.Create(File.OpenRead(fileName)))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name.Equals(AllplanAttributesContainer.ELEMENT_NAME))
                                return AllplanAttributesContainer.ReadFrom(reader);
                            break;                            
                    }
                }

                throw new NotSupportedException("Unexpected end of file");
            }
        }

        public static AllplanAttributesContainer Create(IEnumerable<AttributeDefinition> attributeDefinitions, string version)
        {
            return new AllplanAttributesContainer
            {
                Version = version,
                FileType = "Attribute-Favorit (*.atfanfx)",
                AttributeSet = new AllplanAttributeSet
                {
                    Key = "0 0 0 0 0 0 0 0",
                    Attributes = attributeDefinitions.Select(a => new AllplanAttribute { Ifnr = a.Ifnr, Suffix = a.Datatype, Value = "" }).ToList()
                }
            };
        }

        public AllplanAttributesContainer FilterByIfNr(long[] ifnrArray)
        {
            Array.Sort(ifnrArray);
            return new AllplanAttributesContainer
            {
                Version = Version,
                FileType = FileType,
                AttributeSet = new AllplanAttributeSet
                {
                    Key = AttributeSet.Key,
                    Attributes = AttributeSet.Attributes.Where(a => -1 < Array.BinarySearch(ifnrArray, a.Ifnr)).ToList()
                }
            };
        }

        public AllplanAttributesContainer ExcludeByIfNr(long[] ifnrArray)
        {
            Array.Sort(ifnrArray);
            return new AllplanAttributesContainer
            {
                Version = Version,
                FileType = FileType,
                AttributeSet = new AllplanAttributeSet
                {
                    Key = AttributeSet.Key,
                    Attributes = AttributeSet.Attributes.Where(a => 0 > Array.BinarySearch(ifnrArray, a.Ifnr)).ToList()
                }
            };
        }

        internal static AllplanAttributesContainer ReadFrom(XmlReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element)
                throw new NotSupportedException($"Expecting element type");
            if (!reader.Name.Equals(ELEMENT_NAME))
                throw new NotSupportedException($"Expecting '{ELEMENT_NAME}' as prefix. Got '{reader.Name}'");

            var container = new AllplanAttributesContainer
            {
                Version = reader.GetAttribute("VERSION"),
                FileType = reader.GetAttribute("FILETYPE")
            };

            while(reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.Equals(AllplanAttributeSet.ELEMENT_NAME))
                            container.AttributeSet = AllplanAttributeSet.ReadFrom(reader);
                        else
                            throw new NotImplementedException($"Missing implementation for element '{reader.Name}'");
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name.Equals(ELEMENT_NAME))
                            return container;
                        break;
                }
            }
            throw new NotSupportedException($"Unexpected end of file");
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public class AllplanAttributeSet
    {
        internal const string ELEMENT_NAME = "NEM_ATTRIB_SET";

        public string Key { get; set; }

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

    [IsVisibleInDynamoLibrary(false)]
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

#pragma warning restore CS1591 // Missing comments only public methods / accessors