using System.Collections.Generic;

//#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IPortDataContainer
    {
        IList<PortData> InputPorts { get; }
        IList<PortData> OutputPorts { get; }
    }
}
