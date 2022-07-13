using System;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Path/Constant Path")]
    sealed class ConstPathNode : INodeProcessor
    {
        ConstPathNode() { }

        [SerializeField]
        string m_Path = string.Empty;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            container.Set(portDataContainer.OutputPorts[0], new PathData(asset => m_Path));
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
