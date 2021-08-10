using UnityEngine;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    interface IGraphViewCallbackReceiver
    {
        void AddElement(GraphElement graphElement, Vector2 screenMousePosition);
    }
}
