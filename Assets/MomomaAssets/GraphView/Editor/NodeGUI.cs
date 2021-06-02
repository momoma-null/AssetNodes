using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class NodeGUI : Node, IFieldHolder, ISelectableCallback
    {
        readonly INodeData m_Node;

        public IGraphElementData GraphElementData => m_Node;

        public event Action<GraphElement>? onSelected;

        public NodeGUI(INodeData nodeData)
        {
            m_Node = nodeData;
            style.minWidth = 150f;
            extensionContainer.style.backgroundColor = new Color(0.1803922f, 0.1803922f, 0.1803922f, 0.8039216f);
            title = m_Node.Title;
            foreach (var data in m_Node.InputPorts)
            {
                var port = Port.Create<AdvancedEdge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, data.PortType);
                if (!string.IsNullOrEmpty(data.PortName))
                    port.portName = data.PortName;
                inputContainer.Add(port);
            }
            foreach (var data in m_Node.OutputPorts)
            {
                var port = Port.Create<AdvancedEdge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, data.PortType);
                if (!string.IsNullOrEmpty(data.PortName))
                    port.portName = data.PortName;
                inputContainer.Add(port);
            }
            RefreshPorts();
            RefreshExpandedState();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            onSelected?.Invoke(this);
        }

        public void Bind(SerializedObject serializedObject)
        {
            using (var parentProperty = serializedObject.FindProperty("m_GraphElementData"))
            {
                foreach (var prop in m_Node.GetProperties())
                {
                    IBindable field = prop switch
                    {
                        PropertyValue<UnityObjectWrapper> i => new ObjectField() { value = i.Value.Target, objectType = i.Value.ObjectType },
                        PropertyValue<string> i => new TextField() { value = i.Value },
                        PropertyValue<float> i => new FloatField() { value = i.Value },
                        PropertyValue<int> i => new IntegerField() { value = i.Value },
                        PropertyValueList i => new ListView() { itemsSource = i.Value },
                        _ => throw new ArgumentOutOfRangeException(nameof(prop))
                    };
                    extensionContainer.Add(field as VisualElement);
                    var sp = parentProperty.FindPropertyRelative(prop.FieldName);
                    field.BindProperty(sp);
                }
            }
            RefreshExpandedState();
        }
    }
}
