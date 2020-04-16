using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using Autodesk.DesignScript.Runtime;

#pragma warning disable CS1591 // Missing comments only public methods / accessors

namespace GGUStratic.Data
{
    [XmlRoot("DrillingProfiles")]
    [IsVisibleInDynamoLibrary(false)]
    public class DrillingProfiles
    {
        public Drilling[] Drillings { get; set; }

        public SoilMainType[] SoilMainTypes { get; set; }

        public static DrillingProfiles ReadFrom(string fileName)
        {
            var serializer = new XmlSerializer(typeof(DrillingProfiles), new Type[] { typeof(Drilling) });
            serializer.UnknownElement += (sender, e) => Debug.WriteLine($"Unknown element {e.Element} @ {e.LineNumber}:{e.LinePosition}");
            serializer.UnknownAttribute += (sender, e) => Debug.WriteLine($"Unknown element {e.Attr} @ {e.LineNumber}:{e.LinePosition}");
            var profiles = serializer.Deserialize(File.OpenRead(fileName)) as DrillingProfiles;
            profiles.RecursiveRegisterModel(
                Enumerable.Concat<DrillingModelElement>(profiles.SoilMainTypes, profiles.Drillings)
            );
            return profiles;
        }

        private void RecursiveRegisterModel(IEnumerable<DrillingModelElement> initialElements)
        {
            var visited = new HashSet<DrillingModelElement>();
            var queue = new Queue<DrillingModelElement>(initialElements);

            while(queue.Count > 0)
            {
                var e = queue.Dequeue();
                if (!visited.Contains(e))
                {
                    visited.Add(e);
                    e.DrillingModel = this;

                    foreach(var prop in e.GetType().GetProperties().Where(p => p.CanWrite && p.PropertyType.IsArray))
                    {
                        if (typeof(DrillingModelElement).IsAssignableFrom(prop.PropertyType.GetElementType()))
                            foreach (var subelement in ((IEnumerable<object>)prop.GetValue(e)).OfType<DrillingModelElement>())
                                queue.Enqueue(subelement);
                    }
                }
            }
        }
    }
}

#pragma warning restore CS1591