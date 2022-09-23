using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    interface IStackNodeData : IGraphElementData
    {
        void InsertElements(int index, IEnumerable<string> guids);
        void RemoveElements(IEnumerable<string> guids);
    }
}
