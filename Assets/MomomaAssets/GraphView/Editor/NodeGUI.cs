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
            using (var dataProperty = serializedObject.FindProperty("m_GraphElementData"))
            using (var endProperty = dataProperty.GetEndProperty(false))
            {
                dataProperty.NextVisible(true);
                while (true)
                {
                    if (SerializedProperty.EqualContents(endProperty, dataProperty))
                        break;
                    var key = dataProperty.propertyPath;
                    if (!m_BoundElements.ContainsKey(key))
                    {
                        var sp = dataProperty.Copy();
                        var field = new PropertyField(sp);
                        extensionContainer.Add(field);
                        RefreshExpandedState();
                        field.BindProperty(sp);
                        m_BoundElements.Add(key, field);
                    }
                    toRemoveElements.Remove(m_BoundElements[key]);
                    if (dataProperty.NextVisible(false))
                        break;
                }
            }
            foreach (var element in toRemoveElements)
                extensionContainer.Remove(element);
            RefreshExpandedState();
        }

        public void OnValueChanged()
        {
            RefreshPorts();
        }

        new void RefreshPorts()
        {
            var ports = inputContainer.Query<Port>().ToList().ToDictionary(i => i.viewDataKey, i => i);
            var portCount = 0;
            foreach (var data in m_Node.InputPorts)
            {
                if (ports.TryGetValue(data.Id, out var port))
                {
                    ports.Remove(data.Id);
                    if (port.portType != data.PortType)
                    {
                        port.portName = "";
                        port.portType = data.PortType;
                    }
                }
                else
                {
                    port = Port.Create<AdvancedEdge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, data.PortType);
                    if (!string.IsNullOrEmpty(data.Id))
                        port.viewDataKey = data.Id;
                }
                if (!string.IsNullOrEmpty(data.PortName))
                    port.portName = data.PortName;
                inputContainer.Insert(portCount++, port);
            }
            foreach (var port in ports.Values)
                inputContainer.Remove(port);
            ports = outputContainer.Query<Port>().ToList().ToDictionary(i => i.viewDataKey, i => i);
            portCount = 0;
            foreach (var data in m_Node.OutputPorts)
            {
                if (ports.TryGetValue(data.Id, out var port))
                {
                    ports.Remove(data.Id);
                    if (port.portType != data.PortType)
                    {
                        port.portName = "";
                        port.portType = data.PortType;
                    }
                }
                else
                {
                    port = Port.Create<AdvancedEdge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, data.PortType);
                    if (!string.IsNullOrEmpty(data.Id))
                        port.viewDataKey = data.Id;
                }
                if (!string.IsNullOrEmpty(data.PortName))
                    port.portName = data.PortName;
                outputContainer.Insert(portCount++, port);
            }
            foreach (var port in ports.Values)
                outputContainer.Remove(port);
            base.RefreshPorts();
        }
    }
}
