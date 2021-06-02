
#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IEdgeData : IGraphElementData
    {
        string InputPortGuid { get; }
        string OutputPortGuid { get; }
    }
}
