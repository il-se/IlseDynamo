using System.IO;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Serialization;

namespace IlseDynamo.Data.Allplan
{
    [XmlRoot(ElementName = "AttributeDefinitionCollection", IsNullable = false)]
    public class AttributeDefinitionCollection
    {
        public string Region { get; set; } = "DE";

        [XmlElement]
        public List<AttributeDefinition> AttributeDefinition { get; set; } = new List<AttributeDefinition>();

        public static AttributeDefinitionCollection ReadFrom(string fileName)
        {
            var serializer = new XmlSerializer(typeof(AttributeDefinitionCollection));
            AttributeDefinitionCollection definitionCollection;
            using (var fileStream = File.OpenRead(fileName))
            {
                definitionCollection = serializer.Deserialize(fileStream) as AttributeDefinitionCollection;
                fileStream.Close();
            }
            return definitionCollection;
        }

        public void SaveTo(string fileName)
        {
            var serializer = new XmlSerializer(typeof(AttributeDefinitionCollection));
            using (var xmlWriter = XmlWriter.Create(
                File.Create(fileName), new XmlWriterSettings { Encoding = System.Text.Encoding.UTF8, CloseOutput = true }))
            {
                serializer.Serialize(xmlWriter, this);
                xmlWriter.Close();
            }        
        }
    }
}
