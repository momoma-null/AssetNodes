using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "IO/Rename")]
    sealed class RenameNode : INodeProcessor
    {
        [SerializeField]
        string m_RegexPattern = string.Empty;
        [SerializeField]
        string m_Replacement = string.Empty;

        RenameNode() { }

        public Color HeaderColor => ColorDefinition.IONode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            var regex = new Regex(m_RegexPattern);
            foreach (var assets in assetGroup)
            {
                var srcPath = assets.AssetPath;
                var directoryPath = Path.GetDirectoryName(srcPath);
                var fileName = Path.GetFileName(srcPath);
                var dstFileName = regex.Replace(fileName, m_Replacement);
                var dstPath = Path.Combine(directoryPath, dstFileName);
                AssetDatabase.MoveAsset(srcPath, dstPath);
            }
            container.SetOutput(0, assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
