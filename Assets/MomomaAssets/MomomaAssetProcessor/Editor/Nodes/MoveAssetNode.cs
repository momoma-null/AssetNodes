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
    [InitializeOnLoad]
    [Serializable]
    sealed class MoveAssetNode : INodeData
    {
        static MoveAssetNode()
        {
            INodeDataUtility.AddConstructor(() => new MoveAssetNode());
        }

        MoveAssetNode() { }

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();
        public string MenuPath => "File/Move Asset";
        public IEnumerable<PortData> InputPorts => new[] { m_InputPort };
        public IEnumerable<PortData> OutputPorts => new[] { m_OutputPort };

        [SerializeField]
        [HideInInspector]
        PortData m_InputPort = new PortData(typeof(UnityObject));

        [SerializeField]
        [HideInInspector]
        PortData m_OutputPort = new PortData(typeof(UnityObject));

        [SerializeField]
        string m_SourcePath = "Assets/(.+)";

        [SerializeField]
        string m_DestinationPath = "Assets/$1";

        public void Process(ProcessingDataContainer container)
        {
            var assetGroup = container.Get(m_InputPort.Id, () => new AssetGroup());
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
            container.Set(m_OutputPort.Id, assetGroup);
        }
    }
}
