using System.Collections.Generic;

using System.Xml;
using System.Xml.Serialization;

namespace IlseDynamo.Data.Allplan
{
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
}
