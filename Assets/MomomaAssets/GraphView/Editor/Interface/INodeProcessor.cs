using System;
using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface INodeProcessor
    {
        IGraphElementEditor GraphElementEditor { get; }
        IEnumerable<PortData> InputPorts { get; }
        IEnumerable<PortData> OutputPorts { get; }
        void Process(ProcessingDataContainer container);
    }
}
