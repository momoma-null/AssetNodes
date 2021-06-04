using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class NodeGUI : Node, IFieldHolder
    {
        readonly INodeData m_Node;
        readonly Dictionary<string, VisualElement> m_BoundElements = new Dictionary<string, VisualElement>();

        public event Action<GraphElement>? onValueChanged;
        public IGraphElementData GraphElementData => m_Node;

        public NodeGUI(INodeData nodeData) : base()
        {
            m_Node = nodeData;
            style.minWidth = 150f;
            extensionContainer.style.backgroundColor = new Color(0.1803922f, 0.1803922f, 0.1803922f, 0.8039216f);
            title = m_Node.Title;
            capabilities |= Capabilities.Renamable;
            RefreshPorts();
        }

        public void Bind(SerializedObject serializedObject)
        {
            var toRemoveElements = new HashSet<VisualElement>(m_BoundElements.Values);
            using (var parentProperty = serializedObject.FindProperty("m_GraphElementData"))
            {
                foreach (var prop in m_Node.GetProperties())
                {
                    var sp = parentProperty.FindPropertyRelative(prop.FieldName);
                    if (!m_BoundElements.ContainsKey(sp.propertyPath))
                    {
                        var field = new PropertyField(sp);
                        extensionContainer.Add(field);
                        RefreshExpandedState();
                        field.BindProperty(sp);
                        field.Query<PropertyField>().ForEach(i => i.RegisterValueChangeCallback(e => RefreshPorts()));
                        m_BoundElements.Add(sp.propertyPath, field);
                    }
                    toRemoveElements.Remove(m_BoundElements[sp.propertyPath]);
                }
            }
            foreach (var element in toRemoveElements)
                extensionContainer.Remove(element);
            RefreshExpandedState();
        }

        new void RefreshPorts()
        {
            var ports = inputContainer.Query<Port>().ToList().ToDictionary(i => i.viewDataKey, i => i);
            var portCount = 0;
            foreach (var data in m_Node.InputPorts)
            {
                if (!ports.Remove(data.Id))
                {
                    var port = Port.Create<AdvancedEdge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, data.PortType);
                    if (!string.IsNullOrEmpty(data.PortName))
                        port.portName = data.PortName;
                    if (!string.IsNullOrEmpty(data.Id))
                        port.viewDataKey = data.Id;
                    inputContainer.Insert(portCount, port);
                }
                ++portCount;
            }
            foreach (var port in ports.Values)
                inputContainer.Remove(port);
            ports = outputContainer.Query<Port>().ToList().ToDictionary(i => i.viewDataKey, i => i);
            portCount = 0;
            foreach (var data in m_Node.OutputPorts)
            {
                if (!ports.Remove(data.Id))
                {
                    var port = Port.Create<AdvancedEdge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, data.PortType);
                    if (!string.IsNullOrEmpty(data.PortName))
                        port.portName = data.PortName;
                    if (!string.IsNullOrEmpty(data.Id))
                        port.viewDataKey = data.Id;
                    outputContainer.Insert(portCount, port);
                }
                ++portCount;
            }
            foreach (var port in ports.Values)
                outputContainer.Remove(port);
            base.RefreshPorts();
            onValueChanged?.Invoke(this);
        }
    }
}
