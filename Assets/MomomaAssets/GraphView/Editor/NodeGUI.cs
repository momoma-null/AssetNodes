using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class NodeGUI : Node, IFieldHolder
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
        }

        public void RegisterFields(IFieldRegister fieldRegister)
        {
            foreach (var prop in m_Node.GetProperties())
            {
                switch (prop)
                {
                    case PropertyValue<UnityObjectWrapper> i:
                        fieldRegister.RegisterFields(new ObjectField() { value = i.Value.Target, objectType = i.Value.ObjectType }); break;
                    case PropertyValue<string> i:
                        fieldRegister.RegisterFields(new TextField() { value = i.Value }); break;
                    case PropertyValue<float> i:
                        fieldRegister.RegisterFields(new FloatField() { value = i.Value }); break;
                    default: throw new InvalidOperationException();
                };
            }
        }
    }
}
