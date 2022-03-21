
//#nullable enable

namespace MomomaAssets.GraphView
{
    interface INodeData : IGraphElementData, IPortDataContainer
    {
        bool Expanded { get; }
        INodeProcessor Processor { get; }
    }
}
