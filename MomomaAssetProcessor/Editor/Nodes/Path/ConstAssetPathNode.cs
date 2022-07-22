using System;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Path/Constant Asset Path")]
    sealed class ConstAssetPathNode : INodeProcessor
    {
        ConstAssetPathNode() { }

        [SerializeField]
        UnityObject? m_Asset = null;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            container.Set(portDataContainer.OutputPorts[0], new PathData(asset => AssetDatabase.GetAssetPath(m_Asset)));
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
