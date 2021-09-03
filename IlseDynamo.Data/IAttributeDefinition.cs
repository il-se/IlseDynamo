using System;

namespace IlseDynamo.Data
{
    public interface IAttributeDefinition
    {
        Guid? DefinitionId { get; }

        long? InstanceId { get; }

        IAttributeScope[] Scopes { get; }

        string Name { get; }
        string Description { get; }

        string InstanceType { get; }
        AttributeTypes Type { get; }
        ControlTypes ControlType { get; }
    }
}
