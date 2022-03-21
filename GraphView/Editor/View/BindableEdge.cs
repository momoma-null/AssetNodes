using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

//#nullable enable

namespace MomomaAssets.GraphView
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;
    sealed class BindableEdge : Edge, IBindableGraphElement
    {
        sealed class PersistentPortData : BindableElement, INotifyValueChanged<string>
        {
            readonly BindableEdge m_Edge;
            readonly bool m_IsInput;

            string m_Guid = "";

            public string value
            {
                get => m_Guid;
                set
                {
                    if (value == m_Guid)
                        return;
                    if (panel != null)
                    {
                        using (var evt = ChangeEvent<string>.GetPooled(m_Guid, value))
                        {
                            evt.target = this;
                            SetValueWithoutNotify(value);
                            SendEvent(evt);
                        }
                    }
                    else
                    {
                        SetValueWithoutNotify(value);
                    }
                }
            }

            public PersistentPortData(BindableEdge edge, bool isInput)
            {
                m_Edge = edge;
                m_IsInput = isInput;
            }

            public void SetValueWithoutNotify(string newValue)
            {
                m_Guid = newValue;
                if (m_Edge.m_GraphView != null)
                {
                    if (m_IsInput)
                    {
                        if (m_Edge.input?.viewDataKey != m_Guid)
                        {
                            m_Edge.input?.Disconnect(m_Edge);
                            m_Edge.input = m_Edge.m_GraphView.GetPortByGuid(m_Guid);
                            m_Edge.input?.Connect(m_Edge);
                        }
                    }
                    else
                    {
                        if (m_Edge.output?.viewDataKey != m_Guid)
                        {
                            m_Edge.output?.Disconnect(m_Edge);
                            m_Edge.output = m_Edge.m_GraphView.GetPortByGuid(m_Guid);
                            m_Edge.output?.Connect(m_Edge);
                        }
                    }
                }
            }
        }

        public BindableEdge() : this(new EdgeData("", "")) { }
        public BindableEdge(IEdgeData edgeData) : base()
        {
            m_EdgeData = edgeData;
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            m_InputPort = new PersistentPortData(this, true);
            m_OutputPort = new PersistentPortData(this, false);
            Add(m_InputPort);
            Add(m_OutputPort);
        }

        readonly IEdgeData m_EdgeData;
        readonly PersistentPortData m_InputPort;
        readonly PersistentPortData m_OutputPort;
        GraphView m_GraphView;

        public IGraphElementData GraphElementData => m_EdgeData;

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_GraphView = GetFirstAncestorOfType<GraphView>();
            if (!isGhostEdge && input != null && output != null)
            {
                m_InputPort.value = input.viewDataKey;
                m_OutputPort.value = output.viewDataKey;
            }
        }

        public void Bind(SerializedObject serializedObject)
        {
            serializedObject.Update();
            var inputPortProperty = serializedObject.FindProperty("m_GraphElementData.m_InputPortGuid");
            if (string.IsNullOrEmpty(inputPortProperty.stringValue))
                inputPortProperty.stringValue = input?.viewDataKey;
            m_InputPort.BindProperty(inputPortProperty);
            var outputPortProperty = serializedObject.FindProperty("m_GraphElementData.m_OutputPortGuid");
            if (string.IsNullOrEmpty(outputPortProperty.stringValue))
                outputPortProperty.stringValue = output?.viewDataKey;
            m_OutputPort.BindProperty(outputPortProperty);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
