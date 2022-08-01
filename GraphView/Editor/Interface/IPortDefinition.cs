using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IPortDefinition<T> where T : IProcessingData
    {
        bool IsMultiInput { get; }
        bool IsMultiOutput { get; }
        Color PortColor { get; }
        T CombineInputData(IEnumerable<T> inputs);
    }
}
