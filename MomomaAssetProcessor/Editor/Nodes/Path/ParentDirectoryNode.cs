using System;
using System.IO;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Path/Get Parent Directory")]
    sealed class ParentDirectoryNode : INodeProcessor
    {
        ParentDirectoryNode() { }

        [SerializeField]
        uint m_Count = 1u;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<string>();
            portDataContainer.AddOutputPort<string>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var pathData = container.Get(portDataContainer.InputPorts[0], PathData.combine);
            var outpathData = new PathData(asset =>
            {
                var path = pathData.GetPath(asset);
                for (var i = 0; i < m_Count; ++i)
                    path = Path.GetDirectoryName(path);
                return path;
            });
            container.Set(portDataContainer.OutputPorts[0], outpathData);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
