using System;
using System.IO;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Path/Get Child Folder")]
    sealed class GetChildFolderNode : INodeProcessor
    {
        [SerializeField]
        string m_FolderName = string.Empty;

        GetChildFolderNode() { }

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
                return Path.Combine(path, m_FolderName);
            });
            container.SetOutput(0, outPathData);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
