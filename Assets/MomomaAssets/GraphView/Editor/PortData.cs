using System;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class PortData
    {
        public Type PortType { get; }
        public string PortName { get; }

        public PortData(Type type, string name = "")
        {
            PortType = type;
            PortName = name;
        }
    }
}
