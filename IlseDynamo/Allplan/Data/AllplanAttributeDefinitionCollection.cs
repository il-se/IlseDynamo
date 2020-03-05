using System;
using System.IO;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Serialization;

using Autodesk.DesignScript.Runtime;

namespace Allplan.Data
{
    [XmlRoot(ElementName = "AttributeDefinitionCollection", IsNullable = false)]
    [IsVisibleInDynamoLibrary(false)]
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

    [IsVisibleInDynamoLibrary(false)]
    public class AttributeDefinition
    {
        [XmlElement]
        public Guid Uid { get; set; } = Guid.NewGuid();
        [XmlElement]
        public long Ifnr { get; set; } = -1;
        [XmlElement]
        public string Text { get; set; } = "";
        [XmlElement]
        public double MinValue { get; set; } = 0;
        [XmlElement]
        public double MaxValue { get; set; } = 60;
        [XmlElement]
        public string Datatype { get; set; } = "C";
        [XmlElement]
        public long Group { get; set; } = 1;
        [XmlElement]
        public string Modify { get; set; } = "true";
        [XmlElement]
        public string Visible { get; set; } = "true";
        [XmlElement]
        public string FunctionPointer { get; set; } = "FUNC:0;";
        [XmlElement]
        public long PropertyBitMask { get; set; } = 0;
        [XmlElement]
        public long ParentUserDirCode { get; set; } = -1;

        [XmlElement]
        public ComboBoxInput ComboBox { get; set; } = null;

        [XmlElement]
        public string TextBox { get; set; } = null;

        public override bool Equals(object obj)
        {
            return obj is AttributeDefinition definition &&
                   Uid.Equals(definition.Uid) &&
                   Ifnr == definition.Ifnr &&
                   Text == definition.Text &&
                   MinValue == definition.MinValue &&
                   MaxValue == definition.MaxValue &&
                   Datatype == definition.Datatype &&
                   Group == definition.Group &&
                   Modify == definition.Modify &&
                   Visible == definition.Visible &&
                   FunctionPointer == definition.FunctionPointer &&
                   PropertyBitMask == definition.PropertyBitMask &&
                   ParentUserDirCode == definition.ParentUserDirCode &&
                   EqualityComparer<ComboBoxInput>.Default.Equals(ComboBox, definition.ComboBox) &&
                   TextBox == definition.TextBox;
        }

        public override int GetHashCode()
        {
            var hashCode = -2125747154;
            hashCode = hashCode * -1521134295 + Uid.GetHashCode();
            hashCode = hashCode * -1521134295 + Ifnr.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + MinValue.GetHashCode();
            hashCode = hashCode * -1521134295 + MaxValue.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Datatype);
            hashCode = hashCode * -1521134295 + Group.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Modify);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Visible);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FunctionPointer);
            hashCode = hashCode * -1521134295 + PropertyBitMask.GetHashCode();
            hashCode = hashCode * -1521134295 + ParentUserDirCode.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<ComboBoxInput>.Default.GetHashCode(ComboBox);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TextBox);
            return hashCode;
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public class ComboBoxInput
    {
        [XmlElement]
        public ComboBoxItem[] Item { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ComboBoxInput input)
            {
                if (Item == input.Item)
                    return true;

                if (Item?.Length != input.Item?.Length)
                    return false;

                for (int i=0; i<Item.Length; i++)
                {
                    if (!Item[i].Equals(input.Item[i]))
                        return false;
                }

                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return -979861770 + EqualityComparer<ComboBoxItem[]>.Default.GetHashCode(Item);
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public class ComboBoxItem
    {
        [XmlElement]
        public string Key { get; set; }
        [XmlElement]
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ComboBoxItem item &&
                   Key == item.Key &&
                   Value == item.Value;
        }

        public override int GetHashCode()
        {
            var hashCode = 206514262;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }
    }
}
