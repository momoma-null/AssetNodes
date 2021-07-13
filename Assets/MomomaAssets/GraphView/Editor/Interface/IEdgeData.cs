
#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IEdgeData : IGraphElementData
    {
        string InputPortGuid { get; set; }
        string OutputPortGuid { get; set; }
    }
}
