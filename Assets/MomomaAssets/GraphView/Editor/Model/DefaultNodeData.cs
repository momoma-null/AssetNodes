using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;

    [Serializable]
    sealed class DefaultNodeData : INodeData
    {
        [SerializeField]
        bool m_Expanded = true;
        [SerializeField]
        List<PortData> m_InputPorts;
        [SerializeField]
        List<PortData> m_OutputPorts;
        [SerializeReference]
        INodeProcessor m_Processor;

        NodeDataEditor? m_NodeDataEditor;

        public int Priority => 0;
        public IGraphElementEditor GraphElementEditor => m_NodeDataEditor ?? (m_NodeDataEditor = new NodeDataEditor(m_Processor.GraphElementEditor));
        public bool Expanded => m_Expanded;
        public INodeProcessor Processor => m_Processor;
        public List<PortData> InputPorts => m_InputPorts;
        public List<PortData> OutputPorts => m_OutputPorts;

        public DefaultNodeData(INodeProcessor processor)
        {
            m_InputPorts = new List<PortData>();
            m_OutputPorts = new List<PortData>();
            m_Processor = processor;
            m_Processor.Initialize(this);
        }

        public GraphElement Deserialize()
        {
            var node = new NodeGUI(this);
            PortDataToPort(InputPorts, node.inputContainer.Query<Port>().ToList());
            PortDataToPort(OutputPorts, node.outputContainer.Query<Port>().ToList());
            node.expanded = m_Expanded;
            return node;
        }

        public void DeserializeOverwrite(GraphElement graphElement, GraphView graphView)
        {
            if (!(graphElement is Node node))
                throw new InvalidOperationException();
            var toDeleteElements = new HashSet<GraphElement>();
            var ports = node.inputContainer.Query<Port>().ToList().ToDictionary(i => i.viewDataKey, i => i);
            var portCount = 0;
            foreach (var data in InputPorts)
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
                    port = Port.Create<DefaultEdge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, data.PortType);
                    if (!string.IsNullOrEmpty(data.Id))
                        port.viewDataKey = data.Id;
                }
                if (!string.IsNullOrEmpty(data.PortName))
                    port.portName = data.PortName;
                node.inputContainer.Insert(portCount++, port);
            }
            foreach (var port in ports.Values)
            {
                toDeleteElements.UnionWith(port.connections);
                node.inputContainer.Remove(port);
            }
            ports = node.outputContainer.Query<Port>().ToList().ToDictionary(i => i.viewDataKey, i => i);
            portCount = 0;
            foreach (var data in OutputPorts)
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
                    port = Port.Create<DefaultEdge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, data.PortType);
                    if (!string.IsNullOrEmpty(data.Id))
                        port.viewDataKey = data.Id;
                }
                if (!string.IsNullOrEmpty(data.PortName))
                    port.portName = data.PortName;
                node.outputContainer.Insert(portCount++, port);
            }
            foreach (var port in ports.Values)
            {
                toDeleteElements.UnionWith(port.connections);
                node.outputContainer.Remove(port);
            }
            node.RefreshPorts();
            node.expanded = m_Expanded;
            graphView.DeleteElements(toDeleteElements);
        }

        static void PortDataToPort(IEnumerable<PortData> portDatas, IEnumerable<Port> ports)
        {
            using (var e1 = portDatas.GetEnumerator())
            using (var e2 = ports.GetEnumerator())
                while (e1.MoveNext() && e2.MoveNext())
                    e2.Current.viewDataKey = e1.Current.Id;
        }

        public void ReplaceGuid(Dictionary<string, string> guids)
        {
            foreach (var i in InputPorts)
            {
                if (!guids.TryGetValue(i.Id, out var newGuid))
                {
                    newGuid = PortData.GetNewId();
                    guids.Add(i.Id, newGuid);
                }
                i.Id = newGuid;
            }
            foreach (var i in OutputPorts)
            {
                if (!guids.TryGetValue(i.Id, out var newGuid))
                {
                    newGuid = PortData.GetNewId();
                    guids.Add(i.Id, newGuid);
                }
                i.Id = newGuid;
            }
        }

        sealed class NodeDataEditor : IGraphElementEditor
        {
            readonly IGraphElementEditor m_ProcessorEditor;

            public NodeDataEditor(IGraphElementEditor processorEditor)
            {
                m_ProcessorEditor = processorEditor;
            }

            public bool UseDefaultVisualElement => m_ProcessorEditor.UseDefaultVisualElement;

            public void OnDestroy()
            {
                m_ProcessorEditor.OnDestroy();
            }

            public void OnGUI(SerializedProperty property)
            {
                using (var m_ProcessorProperty = property.FindPropertyRelative(nameof(m_Processor)))
                {
                    m_ProcessorEditor.OnGUI(m_ProcessorProperty);
                }
            }
        }
    }
}
