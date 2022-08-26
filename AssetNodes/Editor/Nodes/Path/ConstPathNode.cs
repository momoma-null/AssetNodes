using System;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Path/Constant Path")]
    sealed class ConstPathNode : INodeProcessor
    {
        ConstPathNode() { }

        [SerializeField]
        string m_Path = string.Empty;

        public Color HeaderColor => ColorDefinition.PathNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            container.SetOutput(0, new PathData(asset => m_Path));
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
