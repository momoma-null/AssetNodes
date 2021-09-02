using System;
using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class ProcessingDataContainer
    {
        readonly Dictionary<string, IProcessingData> m_ProcessingDatas = new Dictionary<string, IProcessingData>();
        readonly Action<string, ProcessingDataContainer> m_GetAction;
        readonly IReadOnlyDictionary<string, HashSet<string>> m_InputToPreNodes;
        readonly IReadOnlyDictionary<string, HashSet<string>> m_OutputToInputs;

        public ProcessingDataContainer(Action<string, ProcessingDataContainer> getAction, IReadOnlyDictionary<string, HashSet<string>> inputToPreNodes, IReadOnlyDictionary<string, HashSet<string>> outputToInputs)
        {
            m_GetAction = getAction;
            m_InputToPreNodes = inputToPreNodes;
            m_OutputToInputs = outputToInputs;
        }

        public void Set<T>(PortData portData, T data) where T : IProcessingData
        {
            var id = portData.Id;
            m_ProcessingDatas[id] = data;
            if (m_OutputToInputs.TryGetValue(id, out var inputs))
            {
                foreach (var i in inputs)
                    m_ProcessingDatas[i] = data;
            }
        }

        public T Get<T>(PortData portData, Func<T> defaultValue, Func<T, T> copyValue) where T : IProcessingData
        {
            var id = portData.Id;
            if (m_ProcessingDatas.TryGetValue(id, out var data) && data is T t1)
                return copyValue(t1);
            if (m_InputToPreNodes.TryGetValue(id, out var preNodes))
                foreach (var preNode in preNodes)
                    m_GetAction(preNode, this);
            if (m_ProcessingDatas.TryGetValue(id, out data) && data is T t2)
                return copyValue(t2);
            var t3 = defaultValue();
            m_ProcessingDatas[id] = t3;
            return t3;
        }
    }
}
