using System;
using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class ProcessingDataContainer
    {
        readonly Dictionary<string, Delegate> m_Functions = new Dictionary<string, Delegate>();

        public void Set<T>(string id, T data)
        {
            Func<T> func = () => data;
            m_Functions[id] = func;
        }

        public T? Get<T>(string id) where T : class
        {
            if (m_Functions.TryGetValue(id, out var func))
            {
                if (func is Func<T> ret)
                    return ret();
            }
            return default(T);
        }
    }
}
