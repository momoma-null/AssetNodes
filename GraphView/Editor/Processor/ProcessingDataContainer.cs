using System;
using System.Collections.Generic;

//#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class ProcessingDataContainer
    {
        readonly Dictionary<string, IProcessingData> m_ProcessingDatas = new Dictionary<string, IProcessingData>();
        readonly Action<INodeData, ProcessingDataContainer> getData;
        readonly IReadOnlyDictionary<string, HashSet<string>> m_InputPortToOutputPorts;
        readonly IReadOnlyDictionary<string, INodeData> m_OutputPortToNodeData;

        internal IReadOnlyCollection<INodeData> EndNodeDatas { get; }

        internal ProcessingDataContainer(Action<INodeData, ProcessingDataContainer> getData, IReadOnlyDictionary<string, ISerializedGraphElement> guidToSerializedGraphElements)
        {
            this.getData = getData;
            var outputPortToNodeData = new Dictionary<string, INodeData>();
            foreach (var i in guidToSerializedGraphElements.Values)
            {
                if (i.GraphElementData is INodeData nodeData)
                {
                    foreach (var port in nodeData.OutputPorts)
                    {
                        outputPortToNodeData[port.Id] = nodeData;
                    }
                }
            }
            var connectedOutputPorts = new HashSet<string>();
            var inputPortToOutputPorts = new Dictionary<string, HashSet<string>>();
            foreach (var i in guidToSerializedGraphElements.Values)
            {
                if (i.GraphElementData is IEdgeData edgeData)
                {
                    connectedOutputPorts.Add(edgeData.OutputPortGuid);
                    if (!inputPortToOutputPorts.TryGetValue(edgeData.InputPortGuid, out var outputPorts))
                    {
                        outputPorts = new HashSet<string>();
                        inputPortToOutputPorts.Add(edgeData.InputPortGuid, outputPorts);
                    }
                    outputPorts.Add(edgeData.OutputPortGuid);
                }
            }
            var endNodeDatas = new HashSet<INodeData>();
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
                        endNodeDatas.Add(nodeData);
                }
            }
            m_OutputPortToNodeData = outputPortToNodeData;
            m_InputPortToOutputPorts = inputPortToOutputPorts;
            EndNodeDatas = endNodeDatas;
        }

        internal void Clear()
        {
            m_ProcessingDatas.Clear();
        }

        public void Set<T>(PortData portData, T data) where T : IProcessingData
        {
            m_ProcessingDatas[portData.Id] = data;
        }

        public T Get<T>(PortData portData, IPortDefinition<T> portDefinition) where T : IProcessingData
        {
            var id = portData.Id;
            if (m_ProcessingDatas.TryGetValue(id, out var data) && data is T t1)
                return t1;
            var oDatas = new List<T>();
            if (m_InputPortToOutputPorts.TryGetValue(id, out var outputPorts))
            {
                oDatas.Capacity = outputPorts.Count;
                foreach (var o in outputPorts)
                {
                    if (m_ProcessingDatas.TryGetValue(o, out var oData) && oData is T t2)
                    {
                        oDatas.Add(t2);
                    }
                    else
                    {
                        if (m_OutputPortToNodeData.TryGetValue(o, out var nodeData))
                        {
                            getData(nodeData, this);
                            if (m_ProcessingDatas.TryGetValue(o, out oData) && oData is T t3)
                            {
                                oDatas.Add(t3);
                            }
                        }
                    }
                }
            }
            var t4 = portDefinition.CombineInputData(oDatas);
            m_ProcessingDatas[id] = t4;
            return t4;
        }
    }
}
