
#nullable enable

namespace MomomaAssets.GraphView
{
    public interface INodeProcessor : IFunctionProxy<INodeProcessor>
    {
        void Initialize(IPortDataContainer portDataContainer);
        void Process(IProcessingDataContainer container);
    }
}
