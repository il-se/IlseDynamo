using System;
using System.Collections.Generic;

namespace IlseDynamo.Data.Revit
{
    public class RevitParameterGroup : IAttributeScope
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public long? InstanceId { get; set; }

        public string Description { get => null; }

        public override bool Equals(object obj)
        {
            return obj is RevitParameterGroup group &&
                   EqualityComparer<Guid?>.Default.Equals(Id, group.Id) &&
                   Name == group.Name &&
                   InstanceId == group.InstanceId;
        }

        public override int GetHashCode()
        {
            int hashCode = 153520533;
            hashCode = hashCode * -1521134295 + Id?.GetHashCode() ?? 0;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + InstanceId?.GetHashCode() ?? 0;
            return hashCode;
        }
    }
}
