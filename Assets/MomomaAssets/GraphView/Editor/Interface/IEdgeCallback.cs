using System;
using UnityEditor.Experimental.GraphView;

namespace MomomaAssets.GraphView
{
    public interface IEdgeCallback
    {
        event Action<Edge> onPortChanged;
    }
}
