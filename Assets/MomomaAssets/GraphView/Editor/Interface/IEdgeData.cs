
#nullable enable

namespace MomomaAssets.GraphView
{
    interface IEdgeData : IGraphElementData
    {
        string InputPortGuid { get; }
        string OutputPortGuid { get; }
    }
}
