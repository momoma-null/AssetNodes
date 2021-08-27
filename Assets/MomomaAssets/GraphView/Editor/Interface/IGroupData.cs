using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IGroupData : IGraphElementData
    {
        string Name { get; }
        int ElementCount { get; }
        void AddElements(IEnumerable<string> guids);
        void RemoveElements(IEnumerable<string> guids);
    }
}
