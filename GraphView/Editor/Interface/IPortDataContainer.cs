using System.Collections.Generic;

//#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IPortDataContainer
    {
        IReadOnlyList<PortData> InputPorts { get; }
        IReadOnlyList<PortData> OutputPorts { get; }
        void AddInputPort<T>(IPortDefinition<T> portDefinition, string name = "") where T : IProcessingData;
        void AddOutputPort<T>(IPortDefinition<T> portDefinition, string name = "") where T : IProcessingData;
    }
}
