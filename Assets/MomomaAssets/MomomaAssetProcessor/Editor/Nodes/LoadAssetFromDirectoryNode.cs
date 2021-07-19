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

        [SerializeField]
        DefaultAsset? m_Folder;

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.OutputPorts.Add(new PortData(typeof(UnityObject)));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = new AssetGroup();
            if (m_Folder != null)
            {
                var folderPath = AssetDatabase.GetAssetPath(m_Folder);
                var guids = AssetDatabase.FindAssets("", new[] { folderPath });
                var assets = Array.ConvertAll(guids, i => new AssetData(AssetDatabase.GUIDToAssetPath(i)));
                assetGroup = new AssetGroup(assets);
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
