using System;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class PortData
    {
        public Type PortType { get; }
        public string PortName { get; }
        public string Id { get; }

        public PortData(Type type, string name = "", string? id = null)
        {
            PortType = type;
            PortName = name;
            Id = id ?? Guid.NewGuid().ToString();
        }
    }
}
