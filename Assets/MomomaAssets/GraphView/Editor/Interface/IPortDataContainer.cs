using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IPortDataContainer
    {
        List<PortData> InputPorts { get; }
        List<PortData> OutputPorts { get; }
    }
}
