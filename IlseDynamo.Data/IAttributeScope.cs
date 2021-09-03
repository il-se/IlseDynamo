using System;

namespace IlseDynamo.Data
{
    public interface IAttributeScope
    {
        Guid? Id { get; }

        long? InstanceId { get; }

        string Name { get; }

        string Description { get; }
    }
}
