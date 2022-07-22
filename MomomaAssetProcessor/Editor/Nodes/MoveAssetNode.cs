using System;
using System.IO;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "File/Move Asset")]
    sealed class MoveAssetNode : INodeProcessor
    {
        MoveAssetNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroupPortDefinition.Default);
            var path = container.Get(portDataContainer.InputPorts[1], PathDataPortDefinition.Default);
            foreach (var assets in assetGroup)
            {
                var srcPath = assets.AssetPath;
                var dstPath = path.GetPath(assets);
                var directoryPath = Path.GetDirectoryName(dstPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    AssetDatabase.ImportAsset(directoryPath);
                }
                AssetDatabase.MoveAsset(srcPath, dstPath);
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
