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
        public Color HeaderColor => ColorDefinition.PathNode;

        GetChildDirectoryNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(PathDataPortDefinition.Default, "Current Directory");
            portDataContainer.AddInputPort(PathDataPortDefinition.Default, "Child Directory");
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var pathData = container.GetInput(0, PathDataPortDefinition.Default);
            var directory = container.GetInput(1, PathDataPortDefinition.Default);
            var outPathData = new PathData(asset =>
            {
                var path = pathData.GetPath(asset);
                return Path.Combine(path, directory.GetPath(asset));
            });
            container.SetOutput(0, outPathData);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
