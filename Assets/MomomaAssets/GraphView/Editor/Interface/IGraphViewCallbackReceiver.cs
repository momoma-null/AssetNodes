using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    interface IGraphViewCallbackReceiver
    {
        void AddElement(IGraphElementData graphElement, Vector2 screenMousePosition);
    }
}
