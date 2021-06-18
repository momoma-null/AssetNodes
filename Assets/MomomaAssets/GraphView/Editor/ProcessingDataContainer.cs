using System;
using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class ProcessingDataContainer
    {
        readonly Dictionary<string, Delegate> m_Functions = new Dictionary<string, Delegate>();
        readonly Action<string, ProcessingDataContainer> m_GetAction;

        public ProcessingDataContainer(Action<string, ProcessingDataContainer> getAction)
        {
            m_GetAction = getAction;
        }

        public void Set<T>(string id, T data)
        {
            Func<T> func = () => data;
            m_Functions[id] = func;
        }

        public T Get<T>(string id, Func<T> defaultValue) where T : class
        {
            if (m_Functions.TryGetValue(id, out var func))
            {
                if (func is Func<T> ret)
                    return ret();
            }
            m_GetAction(id, this);
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
