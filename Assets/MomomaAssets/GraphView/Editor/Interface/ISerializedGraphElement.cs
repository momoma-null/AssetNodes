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

    public interface ISerializedGraphElement
    {
        string Guid { get; set; }
        string TypeName { get; set; }
        Rect Position { get; set; }
        IReadOnlyList<string> ReferenceGuids { get; set; }
        IGraphElementData? GraphElementData { get; set; }
    }

    public static class SerializedGraphElementExtensions
    {
        public static void Serialize<T>(this GraphElement graphElement, T serializedGraphElement, Rect position) where T : ISerializedGraphElement
        {
            if (graphElement is IFieldHolder fieldHolder)
            {
                serializedGraphElement.GraphElementData = fieldHolder.GraphElementData;
                if (serializedGraphElement is ScriptableObject scriptableObject)
                {
                    var so = new SerializedObject(scriptableObject);
                    fieldHolder.Bind(so);
                }
            }
            serializedGraphElement.Guid = graphElement.viewDataKey;
            serializedGraphElement.TypeName = graphElement.GetType().AssemblyQualifiedName;
            serializedGraphElement.Position = position;
            serializedGraphElement.RebindReferenceGuids();
        }

        public static GraphElement Deserialize<TGraphView>(this ISerializedGraphElement serializedGraphElement, TGraphView graphView) where TGraphView : GraphView, IGraphViewCallback
        {
            GraphElement graphElement = serializedGraphElement.GraphElementData switch
            {
                INodeData nodeData => new NodeGUI(nodeData),
                IEdgeData edgeData => new AdvancedEdge(edgeData),
                _ => throw new ArgumentOutOfRangeException(nameof(serializedGraphElement.GraphElementData))
            };
            graphView.AddElement(graphElement);
            switch (graphElement)
            {
                case Node node:
                    var portCount = 0;
                    node.Query<Port>().ForEach(port => port.viewDataKey = serializedGraphElement.ReferenceGuids[portCount++]);
                    break;
            }
            if (serializedGraphElement is ScriptableObject scriptableObject && graphElement is IFieldHolder fieldHolder)
            {
                var so = new SerializedObject(scriptableObject);
                fieldHolder.Bind(so);
            }
            serializedGraphElement.Deserialize(graphElement);
            return graphElement;
        }

        public static void Deserialize(this ISerializedGraphElement serializedGraphElement, GraphElement graphElement)
        {
            graphElement.viewDataKey = serializedGraphElement.Guid;
            graphElement.SetPosition(serializedGraphElement.Position);
            if (serializedGraphElement.GraphElementData is INodeData nodeData && graphElement is Node node)
            {
                var ports = node.inputContainer.Query<Port>().ToList().ToDictionary(i => i.viewDataKey, i => i);
                var portCount = 0;
                foreach (var data in nodeData.InputPorts)
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
                    node.inputContainer.Insert(portCount++, port);
                }
                foreach (var port in ports.Values)
                    node.inputContainer.Remove(port);
                ports = node.outputContainer.Query<Port>().ToList().ToDictionary(i => i.viewDataKey, i => i);
                portCount = 0;
                foreach (var data in nodeData.OutputPorts)
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
                    node.outputContainer.Insert(portCount++, port);
                }
                foreach (var port in ports.Values)
                    node.outputContainer.Remove(port);
                node.RefreshPorts();
            }
            serializedGraphElement.RebindReferenceGuids();
        }

        static void RebindReferenceGuids(this ISerializedGraphElement serializedGraphElement)
        {
            var referenceGuids = new List<string>();
            switch (serializedGraphElement.GraphElementData)
            {
                case INodeData nodeData:
                    referenceGuids.AddRange(nodeData.InputPorts.Select(i => i.Id));
                    referenceGuids.AddRange(nodeData.OutputPorts.Select(i => i.Id));
                    break;
                case IEdgeData edgeData:
                    referenceGuids.Add(edgeData.InputPortGuid);
                    referenceGuids.Add(edgeData.OutputPortGuid);
                    break;
            }
            serializedGraphElement.ReferenceGuids = referenceGuids;
        }
    }
}
