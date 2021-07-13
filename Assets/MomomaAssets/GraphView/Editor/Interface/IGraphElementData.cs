using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;
    public interface IGraphElementData
    {
        IGraphElementEditor GraphElementEditor { get; }
        GraphElement Deserialize();
        void DeserializeOverwrite(GraphElement graphElement, GraphView graphView);
    }
}
