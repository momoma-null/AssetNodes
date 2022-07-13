using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "IO/Export Unitypackage")]
    sealed class ExportNode : INodeProcessor
    {
        ExportNode() { }

        [SerializeField]
        string m_UnityPackageName = "Export.unitypackage";
        [SerializeField]
        ExportPackageOptions m_Options;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroupPortDefinition.Default);
            if (assetGroup.Count > 0)
            {
                AssetDatabase.ExportPackage(assetGroup.Select(asset => asset.AssetPath).ToArray(), m_UnityPackageName, m_Options);
            }
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
