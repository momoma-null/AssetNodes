using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("File/Move Asset")]
    sealed class MoveAssetNode : INodeProcessor
    {
        static MoveAssetNode()
        {
            INodeDataUtility.AddConstructor(() => new MoveAssetNode());
        }

        MoveAssetNode() { }

        [SerializeField]
        string m_SourcePath = "Assets/(.+)";
        [SerializeField]
        string m_DestinationPath = "Assets/$1";

        public INodeProcessorEditor ProcessorEditor { get; } = new DefaultNodeProcessorEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(UnityObject)));
            portDataContainer.OutputPorts.Add(new PortData(typeof(UnityObject)));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], this.NewAssetGroup);
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
    }
}
