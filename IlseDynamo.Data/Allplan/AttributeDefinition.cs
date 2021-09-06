using System;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Serialization;

namespace IlseDynamo.Data.Allplan
{
    public class AttributeDefinition : IAttributeDefinition
    {
        #region Persistent members

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

        #endregion

        #region IAttribute members

        private ControlTypes controlType;

        [XmlIgnore]
        public Guid? DefinitionId { get => Uid; }

        [XmlIgnore]
        public long? InstanceId { get => Ifnr; }

        [XmlIgnore]
        public string Name { get => Text; }

        [XmlIgnore]
        public string Description { get => Text; }

        [XmlIgnore]
        public string InstanceType { get => Datatype; }

        [XmlIgnore]
        public IAttributeScope[] Scopes { get => new IAttributeScope[] { }; }

        [XmlIgnore]
        public AttributeTypes Type
        {
            get
            {
                switch (Datatype)
                {
                    case "C":
                        return AttributeTypes.String;
                    case "R":
                        return AttributeTypes.Double;
                    case "I":
                        return AttributeTypes.Int;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        [XmlIgnore]
        public ControlTypes ControlType
        {
            get
            {
                if (null == ComboBox)
                    return controlType;
                else
                    return ControlTypes.Combobox;
            }
            set
            {
                switch (controlType = value)
                {
                    case ControlTypes.Combobox:
                        ComboBox = new ComboBoxInput();
                        TextBox = null;
                        break;
                    default:
                        ComboBox = null;
                        TextBox = "";
                        break;
                }
            }
        }

        #endregion

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
                   ParentUserDirCode == definition.ParentUserDirCode;
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
            return hashCode;
        }
    }
}
