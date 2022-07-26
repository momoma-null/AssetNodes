using UnityEditor;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class BindableStackNode : StackNode, IBindableGraphElement
    {
        readonly IStackNodeData m_StackNodeData;

        public IGraphElementData GraphElementData => m_StackNodeData;

        public BindableStackNode(IStackNodeData stackNodeData) : base()
        {
            m_StackNodeData = stackNodeData;
        }

        public void Bind(SerializedObject serializedObject) { }
    }
}
