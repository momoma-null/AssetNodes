using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IGroupData : IGraphElementData
    {
        string Name { get; }
        IEnumerable<string> IncludingGuids { get; }
    }
}
