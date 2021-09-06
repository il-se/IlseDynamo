using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace IlseDynamo.Data.Revit
{
    public class RevitSharedParameter : IAttributeDefinition
    {
        public RevitSharedParameter()
        { }

        public Guid? DefinitionId { get; set; }

        public long? InstanceId { get => null; }

        public RevitParameterGroup Group { get; set; }

        IAttributeScope[] IAttributeDefinition.Scopes { get => new[] { Group }; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string InstanceType { get; set; }

        public AttributeTypes Type => throw new NotImplementedException();

        public ControlTypes ControlType => throw new NotImplementedException();

        public string DataCategory { get; set; }

        public bool IsVisible { get; set; }

        public bool IsModifiable { get; set; }

        public static IEnumerable<RevitSharedParameter> FromTextFile(string parameterFile)
        {
            var groups = new Dictionary<long, RevitParameterGroup>();
            foreach (var line in File.ReadAllLines(parameterFile).Where(r => !string.IsNullOrWhiteSpace(r)))
            {
                var cols = line.Split('\t');
                RevitParameterGroup group;
                long groupId;
                switch (cols[0])
                {
                    case "PARAM":
                        groupId = long.Parse(cols[5]);
                        if (!groups.TryGetValue(groupId, out group))
                        {
                            groups[groupId] = group = new RevitParameterGroup { InstanceId = groupId };
                        }

                        yield return new RevitSharedParameter
                        {
                            DefinitionId = Guid.Parse(cols[1]),
                            Name = cols[2],
                            InstanceType = cols[3],
                            DataCategory = cols[4],
                            Group = group,
                            IsVisible = int.Parse(cols[6]) == 1,
                            Description = cols[7],
                            IsModifiable = int.Parse(cols[8]) == 1
                        };
                        break;
                    case "GROUP":
                        groupId = long.Parse(cols[1]);
                        if (!groups.TryGetValue(groupId, out group))
                        {
                            group = new RevitParameterGroup { InstanceId = groupId };
                        }

                        group.Name = cols[2];
                        groups[groupId] = group;

                        break;
                }
            }
        }
    }
}
