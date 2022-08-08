using System;
using System.IO;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Path/Get Parent Directory")]
    sealed class GetParentDirectoryNode : INodeProcessor
    {
        GetParentDirectoryNode() { }

        [SerializeField]
        uint m_Count = 1u;

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
                for (var i = 0; i < m_Count; ++i)
                    path = Path.GetDirectoryName(path);
                return path;
            });
            container.SetOutput(0, outPathData);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
