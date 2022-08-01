using System;
using System.IO;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Path/Folder Name")]
    sealed class FolderNameNode : INodeProcessor
    {
        FolderNameNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default, "Folder");
        }

        public void Process(IProcessingDataContainer container)
        {
            container.SetOutput(0, new PathData(asset => Path.GetDirectoryName(asset.AssetPath)));
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
