using System.Collections.Generic;

using System.Xml;
using System.Xml.Serialization;

namespace IlseDynamo.Data.Allplan
{
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
