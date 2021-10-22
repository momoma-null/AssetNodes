
#nullable enable

namespace MomomaAssets.GraphView
{
    public interface INodeProcessor
    {
        void Initialize(IPortDataContainer portDataContainer);
        void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer);
    }
}
