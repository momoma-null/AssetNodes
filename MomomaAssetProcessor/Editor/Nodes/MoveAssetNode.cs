using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "File/Move Asset")]
    sealed class MoveAssetNode : INodeProcessor
    {
        MoveAssetNode() { }

        [SerializeField]
        string m_SourcePath = "Assets/(.+)";
        [SerializeField]
        string m_DestinationPath = "Assets/$1";

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroupPortDefinition.Default);
            var regex = new Regex(m_SourcePath);
            foreach (var assets in assetGroup)
            {
                var srcPath = assets.AssetPath;
                var dstPath = regex.Replace(srcPath, m_DestinationPath);
                var directoryPath = Path.GetDirectoryName(dstPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    AssetDatabase.ImportAsset(directoryPath);
                }
                AssetDatabase.MoveAsset(srcPath, dstPath);
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
