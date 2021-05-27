using System;

#nullable enable

namespace MomomaAssets.AssetProcessor
{
    sealed class PortData
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
