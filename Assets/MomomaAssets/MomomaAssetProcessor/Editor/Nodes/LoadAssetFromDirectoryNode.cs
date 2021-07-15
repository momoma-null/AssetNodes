using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Load/From Directory")]
    sealed class LoadAssetFromDirectoryNode : INodeProcessor
    {
        static LoadAssetFromDirectoryNode()
        {
            INodeDataUtility.AddConstructor(() => new LoadAssetFromDirectoryNode());
        }

        LoadAssetFromDirectoryNode() { }

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();
        public IEnumerable<PortData> InputPorts => Array.Empty<PortData>();
        public IEnumerable<PortData> OutputPorts => new[] { m_OutPort };

        [SerializeField]
        [HideInInspector]
        PortData m_OutPort = new PortData(typeof(UnityObject));

        [SerializeField]
        DefaultAsset? m_Folder;

        public void Process(ProcessingDataContainer container)
        {
            var assetGroup = new AssetGroup();
            if (m_Folder != null)
            {
                var folderPath = AssetDatabase.GetAssetPath(m_Folder);
                var guids = AssetDatabase.FindAssets("", new[] { folderPath });
                var assets = Array.ConvertAll(guids, i => new AssetData(AssetDatabase.GUIDToAssetPath(i)));
                assetGroup = new AssetGroup(assets);
            }
            container.Set(m_OutPort, assetGroup);
        }
    }
}
