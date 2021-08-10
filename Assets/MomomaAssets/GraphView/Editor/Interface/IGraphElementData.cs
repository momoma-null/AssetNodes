using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IGraphElementData
    {
        int Priority { get; }
        IGraphElementEditor GraphElementEditor { get; }
        GraphElement Deserialize();
        void DeserializeOverwrite(GraphElement graphElement, UnityEditor.Experimental.GraphView.GraphView graphView);
        void ReplaceGuid(Dictionary<string, string> guids);
    }
}
