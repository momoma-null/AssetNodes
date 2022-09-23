using UnityEngine;

//#nullable enable

namespace MomomaAssets.GraphView
{
    public interface INodeProcessor : IFunctionProxy<INodeProcessor>
    {
        Color HeaderColor { get; }
        void Initialize(IPortDataContainer portDataContainer);
        void Process(IProcessingDataContainer container);
    }
}
