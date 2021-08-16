using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class DefaultEdge : Edge, IEdgeCallback, IFieldHolder
    {
        public DefaultEdge() : this(new DefaultEdgeData("", "")) { }
        public DefaultEdge(IEdgeData edgeData) : base() => m_EdgeData = edgeData;

        readonly IEdgeData m_EdgeData;

        public event Action<Edge>? onPortChanged;
        public IGraphElementData GraphElementData => m_EdgeData;

        public override void OnPortChanged(bool isInput)
        {
            base.OnPortChanged(isInput);
            onPortChanged?.Invoke(this);
        }

        public void Bind(SerializedObject serializedObject) { }
    }
}
