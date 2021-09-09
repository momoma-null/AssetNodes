
//#nullable enable

namespace MomomaAssets.GraphView
{
    public interface INodeProcessor
    {
        INodeProcessorEditor ProcessorEditor { get; }
        void Initialize(IPortDataContainer portDataContainer);
        void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer);
    }
}
