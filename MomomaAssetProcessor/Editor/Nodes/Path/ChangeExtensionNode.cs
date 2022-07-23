using System;
using System.IO;
using UnityEngine;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Path/Change Extension")]
    sealed class ChangeExtensionNode : INodeProcessor
    {
        ChangeExtensionNode() { }

        [SerializeField]
        string m_Extension = ".asset";

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var pathData = container.GetInput(0, PathDataPortDefinition.Default);
            container.SetOutput(0, new PathData(asset => Path.ChangeExtension(pathData.GetPath(asset), m_Extension)));
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
