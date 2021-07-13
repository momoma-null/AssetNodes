using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [Serializable]
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
        public IEnumerable<PortData> OutputPorts => m_OutPorts;

        [SerializeField]
        [HideInInspector]
        PortData[] m_OutPorts = new[] { new PortData(typeof(UnityObject)) };

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
            container.Set(m_OutPorts[0].Id, assetGroup);
        }
    }
}
