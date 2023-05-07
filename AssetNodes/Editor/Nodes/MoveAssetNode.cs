using System;
using System.IO;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "IO/Move Asset")]
    sealed class MoveAssetNode : INodeProcessor
    {
        MoveAssetNode() { }

        public Color HeaderColor => ColorDefinition.IONode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddInputPort(PathDataPortDefinition.Default, "Destination Directory");
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            var path = container.GetInput(1, PathDataPortDefinition.Default);
            var dstAssets = new AssetGroup();
            container.SetOutput(0, dstAssets);
            if (assetGroup.Count == 0)
                return;
            using (new AssetModificationScope())
            {
                foreach (var assets in assetGroup)
                {
                    var directoryPath = path.GetPath(assets);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                        AssetDatabase.ImportAsset(directoryPath);
                    }
                }
            }
            using (new AssetModificationScope())
            {
                foreach (var assets in assetGroup)
                {
                    var srcPath = assets.AssetPath;
                    var directoryPath = path.GetPath(assets);
                    var dstPath = Path.Combine(directoryPath, Path.GetFileName(srcPath));
                    AssetDatabase.MoveAsset(srcPath, dstPath);
                    dstAssets.Add(new AssetData(dstPath));
                }
            }
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
