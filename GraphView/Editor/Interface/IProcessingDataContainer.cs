
//#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IProcessingDataContainer
    {
        T GetInput<T>(int index, IPortDefinition<T> portDefinition) where T : IProcessingData;
        void SetOutput<T>(int index, T data) where T : IProcessingData;
    }
}
