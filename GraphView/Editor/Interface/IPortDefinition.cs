using System;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IPortDefinition<T> where T : IProcessingData
    {
        bool IsMultiInput { get; }
        bool IsMultiOutput { get; }
        Type DisplayType { get; }
    }
}
