using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

using Autodesk.DesignScript.Runtime;

namespace GGUStratic
{
    [IsVisibleInDynamoLibrary(false)]
    public abstract class DrillingModelElement 
    {
        [XmlAttribute(attributeName: "id")]
        public string ID { get; set; }

        protected delegate IEnumerable<DrillingModelElement> ElementGetter(DrillingProfiles root);

        [XmlIgnore]
        protected List<Tuple<string, ElementGetter>> BackedGuidListAttribute { get; set; } = new List<Tuple<string, ElementGetter>>();

        [XmlIgnore]
        internal DrillingProfiles DrillingModel { get; set; }

        protected DrillingModelElement()
        {

        }

        protected ElementGetter GetElementGetter(string propName)
        {
            return BackedGuidListAttribute.FirstOrDefault(a => a.Item1.Equals(propName)).Item2;
        }

        protected void Register(string propName, ElementGetter elementGetterDelegate)
        {
            BackedGuidListAttribute.Add(new Tuple<string, ElementGetter>(propName, elementGetterDelegate));
        }

        protected IEnumerable<T> Filter<T>(string propName, IEnumerable<T> elements) where T : DrillingModelElement
        {
            var serialized = GetType().GetProperty(propName).GetValue(this);
            if(serialized is string s)
            {
                var set = ImmutableHashSet.CreateRange(s.Split(new char[] { ';' }));
                return elements.Where(e => set.Contains(e.ID));
            }
            return Enumerable.Empty<T>();
        }

        protected IEnumerable<T> Get<T>(string propName) where T : DrillingModelElement
        {
            var getter = GetElementGetter(propName);
            if (null != getter)
                return Filter(propName, getter?.Invoke(DrillingModel) as IEnumerable<T>);
            else
                return Enumerable.Empty<T>();
        }
    }
}
