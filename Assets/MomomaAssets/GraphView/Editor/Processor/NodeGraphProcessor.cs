using System;
using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class NodeGraphProcessor
    {
        readonly Action? PreProcess;
        readonly Action? PostProcess;
        readonly Action? Completed;

        public NodeGraphProcessor(Action? preProcess = null, Action? postProcess = null, Action? completed = null)
        {
            PreProcess = preProcess;
            PostProcess = postProcess;
            Completed = completed;
        }

        public void StartProcess(GraphViewObject graphViewObject)
        {
            var portToNode = new Dictionary<string, string>();
            var guidToSerializedGraphElements = (graphViewObject as ISerializedGraphView).SerializedGraphElements.Where(i => i != null && !string.IsNullOrEmpty(i.Guid)).ToDictionary(i => i.Guid, i => i as ISerializedGraphElement);
            foreach (var i in guidToSerializedGraphElements.Values)
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
            foreach (var i in guidToSerializedGraphElements.Values)
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
            foreach (var i in guidToSerializedGraphElements.Values)
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
            var container = new ProcessingDataContainer((i, j) => GetData(guidToSerializedGraphElements, i, j), inputToPreNodes, outputToInputs);
            foreach (var nordId in endNodes)
            {
                GetData(guidToSerializedGraphElements, nordId, container);
            }
            Completed?.Invoke();
        }

        void GetData(Dictionary<string, ISerializedGraphElement> guidToSerializedGraphElements, string id, ProcessingDataContainer container)
        {
            if (guidToSerializedGraphElements[id].GraphElementData is INodeData nodeData)
            {
                try
                {
                    PreProcess?.Invoke();
                    nodeData.Processor.Process(container, nodeData);
                }
                finally
                {
                    PostProcess?.Invoke();
                }
            }
        }
    }
}
