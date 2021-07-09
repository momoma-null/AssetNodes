using System;
using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class ProcessingDataContainer
    {
        readonly Dictionary<string, Delegate> m_Functions = new Dictionary<string, Delegate>();
        readonly Action<string, ProcessingDataContainer> m_GetAction;
        readonly IReadOnlyDictionary<string, HashSet<string>> m_InputToPreNodes;
        readonly IReadOnlyDictionary<string, HashSet<string>> m_OutputToInputs;

        public ProcessingDataContainer(Action<string, ProcessingDataContainer> getAction, IReadOnlyDictionary<string, HashSet<string>> inputToPreNodes, IReadOnlyDictionary<string, HashSet<string>> outputToInputs)
        {
            m_GetAction = getAction;
            m_InputToPreNodes = inputToPreNodes;
            m_OutputToInputs = outputToInputs;
        }

        public void Set<T>(string id, T data)
        {
            Func<T> func = () => data;
            m_Functions[id] = func;
            if (m_OutputToInputs.TryGetValue(id, out var inputs))
            {
                foreach (var input in inputs)
                    m_Functions[input] = func;
            }
        }

        public T Get<T>(string id, Func<T> defaultValue) where T : class
        {
            if (m_Functions.TryGetValue(id, out var func))
            {
                if (func is Func<T> ret)
                    return ret();
            }
            if (m_InputToPreNodes.TryGetValue(id, out var preNodes))
            {
                foreach (var preNode in preNodes)
                    m_GetAction(preNode, this);
            }
            if (m_Functions.TryGetValue(id, out func))
            {
                if (func is Func<T> ret)
                    return ret();
            }
            m_Functions[id] = defaultValue;
            return defaultValue();
        }
    }
}
