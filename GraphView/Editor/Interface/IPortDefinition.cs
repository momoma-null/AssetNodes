using System;
using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IPortDefinition<T> where T : IProcessingData
    {
        bool IsMultiInput { get; }
        bool IsMultiOutput { get; }
        T CombineInputData(IEnumerable<T> inputs);
    }
}
