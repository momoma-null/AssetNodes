using System;
using System.IO;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Path/Asset File Name")]
    sealed class AssetFileNameNode : INodeProcessor
    {
        AssetFileNameNode() { }

        public Color HeaderColor => ColorDefinition.PathNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default, "File Name");
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
