using System.Collections.Generic;

//#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IPortDataContainer
    {
        IReadOnlyList<PortData> InputPorts { get; }
        IReadOnlyList<PortData> OutputPorts { get; }
        void AddInputPort<T>(string name = "", bool isMulti = false);
        void AddOutputPort<T>(string name = "", bool isMulti = false);
    }
}
