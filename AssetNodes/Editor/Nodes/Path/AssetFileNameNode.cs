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
        [SerializeField]
        bool m_WithoutExtension;

        AssetFileNameNode() { }

        public Color HeaderColor => ColorDefinition.PathNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default, "File Name");
        }

        public void Process(IProcessingDataContainer container)
        {
            container.SetOutput(0, new PathData(asset => m_WithoutExtension ? Path.GetFileNameWithoutExtension(asset.AssetPath) : Path.GetFileName(asset.AssetPath)));
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
