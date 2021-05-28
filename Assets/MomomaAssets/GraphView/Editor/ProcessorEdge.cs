using System;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class AdvancedEdge : Edge, IEdgeCallback
    {
        public event Action<Edge>? onPortChanged;

        public AdvancedEdge() : base() { }

        public override void OnPortChanged(bool isInput)
        {
            base.OnPortChanged(isInput);
            onPortChanged?.Invoke(this);
        }
    }
}
