using System;
using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class NodeGraphProcessor
    {
        readonly Action? PreProcess;
        readonly Action? PostProcess;

        public NodeGraphProcessor(Action? preProcess = null, Action? postProcess = null)
        {
            PreProcess = preProcess;
            PostProcess = postProcess;
        }

        public void StartProcess(GraphViewObject graphViewObject)
        {
            var portToNode = new Dictionary<string, string>();
            foreach (var i in graphViewObject.GuidToSerializedGraphElements.Values)
            {
                if (i.GraphElementData is INodeData nodeData)
                {
                    foreach (var port in nodeData.OutputPorts)
                    {
                        portToNode[port.Id] = i.Guid;
                    }
                }
            }
            var inputToPreNodes = new Dictionary<string, HashSet<string>>();
            var outputToInputs = new Dictionary<string, HashSet<string>>();
            var connectedOutputPorts = new HashSet<string>();
            foreach (var i in graphViewObject.GuidToSerializedGraphElements.Values)
            {
                if (i.GraphElementData is IEdgeData edgeData)
                {
                    if (!inputToPreNodes.TryGetValue(edgeData.InputPortGuid, out var preNodes))
                    {
                        preNodes = new HashSet<string>();
                        inputToPreNodes.Add(edgeData.InputPortGuid, preNodes);
                    }
                    preNodes.Add(portToNode[edgeData.OutputPortGuid]);
                    connectedOutputPorts.Add(edgeData.OutputPortGuid);
                    if (!outputToInputs.TryGetValue(edgeData.OutputPortGuid, out var inputs))
                    {
                        inputs = new HashSet<string>();
                        outputToInputs.Add(edgeData.OutputPortGuid, inputs);
                    }
                    inputs.Add(edgeData.InputPortGuid);
                }
            }
            var endNodes = new HashSet<string>();
            foreach (var i in graphViewObject.GuidToSerializedGraphElements.Values)
            {
                if (i.GraphElementData is INodeData nodeData)
                {
                    var isEndNode = true;
                    foreach (var port in nodeData.OutputPorts)
                    {
                        if (connectedOutputPorts.Contains(port.Id))
                        {
                            isEndNode = false;
                            break;
                        }
                    }
                    if (isEndNode)
                        endNodes.Add(i.Guid);
                }
            }
            var container = new ProcessingDataContainer((i, j) => GetData(graphViewObject, i, j), inputToPreNodes, outputToInputs);
            foreach (var nordId in endNodes)
            {
                GetData(graphViewObject, nordId, container);
            }
        }

        void GetData(GraphViewObject graphViewObject, string id, ProcessingDataContainer container)
        {
            if (graphViewObject.GuidToSerializedGraphElements[id].GraphElementData is INodeData nodeData)
            {
                PreProcess?.Invoke();
                nodeData.Process(container);
                PostProcess?.Invoke();
            }
        }
    }
}
