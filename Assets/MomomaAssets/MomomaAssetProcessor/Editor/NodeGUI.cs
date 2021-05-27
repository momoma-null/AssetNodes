using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.AssetProcessor
{
    sealed class NodeGUI : Node
    {
        readonly INode m_Node;

        public NodeGUI(INode iNode)
        {
            m_Node = iNode;
            style.minWidth = 150f;
            extensionContainer.style.backgroundColor = new Color(0.1803922f, 0.1803922f, 0.1803922f, 0.8039216f);
            title = m_Node.Title;
            foreach (var data in m_Node.InputPorts)
            {
                var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, data.PortType);
                if (!string.IsNullOrEmpty(data.PortName))
                    port.portName = data.PortName;
                inputContainer.Add(port);
            }
            foreach (var data in m_Node.OutputPorts)
            {
                var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, data.PortType);
                if (!string.IsNullOrEmpty(data.PortName))
                    port.portName = data.PortName;
                inputContainer.Add(port);
            }
            RefreshPorts();
            foreach (var prop in m_Node.GetProperties())
            {
                VisualElement field = prop switch
                {
                    PropertyValue<UnityObjectWrapper> i => new ObjectField() { value = i.Value.Target, objectType = i.Value.ObjectType },
                    PropertyValue<string> i => new TextField() { value = i.Value },
                    PropertyValue<float> i => new FloatField() { value = i.Value },
                    _ => throw new InvalidOperationException()
                };
                extensionContainer.Add(field);
            }
            RefreshExpandedState();
        }
    }
}
