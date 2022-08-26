using System;
using System.IO;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Path/Get Child Directory")]
    sealed class GetChildDirectoryNode : INodeProcessor
    {
        [SerializeField]
        string m_DirectoryName = string.Empty;

        public Color HeaderColor => ColorDefinition.PathNode;

        GetChildDirectoryNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var pathData = container.GetInput(0, PathDataPortDefinition.Default);
            var outPathData = new PathData(asset =>
            {
                var path = pathData.GetPath(asset);
                return Path.Combine(path, m_DirectoryName);
            });
            container.SetOutput(0, outPathData);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
