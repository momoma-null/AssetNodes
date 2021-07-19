using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface INodeProcessor
    {
        IGraphElementEditor GraphElementEditor { get; }
        void Initialize(IPortDataContainer portDataContainer);
        void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer);
    }
}
