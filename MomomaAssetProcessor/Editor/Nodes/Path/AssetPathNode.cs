using System;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Path/Asset Path")]
    sealed class AssetPathNode : INodeProcessor
    {
        AssetPathNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            container.SetOutput(0, new PathData(asset => asset.AssetPath));
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
