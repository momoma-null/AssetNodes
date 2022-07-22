using System;
using System.IO;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Path/File Name")]
    sealed class FileNameNode : INodeProcessor
    {
        FileNameNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            container.SetOutput(0, new PathData(asset => Path.GetFileName(asset.AssetPath)));
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
