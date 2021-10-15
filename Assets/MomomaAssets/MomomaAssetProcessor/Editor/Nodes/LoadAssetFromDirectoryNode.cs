using System;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

//#nullable enable

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
        bool m_AutoReload = false;
        [SerializeField]
        bool m_UseAsTest = false;
        [SerializeField]
        DefaultAsset m_Folder;

        public INodeProcessorEditor ProcessorEditor { get; } = new DefaultNodeProcessorEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddOutputPort<UnityObject>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = new AssetGroup();
            if (!CoreAssetProcessor.IsProcessing || m_AutoReload)
            {
                if (m_Folder != null)
                {
                    var folderPath = AssetDatabase.GetAssetPath(m_Folder);
                    if (CoreAssetProcessor.IsProcessing)
                    {
                        foreach (var path in CoreAssetProcessor.ImportedAssetsPaths)
                        {
                            if (path.StartsWith(folderPath))
                                assetGroup.Add(new AssetData(path));
                        }
                    }
                    else
                    {
                        if (!CoreAssetProcessor.IsTesting || m_UseAsTest)
                        {
                            var guids = AssetDatabase.FindAssets("", new[] { folderPath });
                            var assets = Array.ConvertAll(guids, i => new AssetData(AssetDatabase.GUIDToAssetPath(i)));
                            assetGroup.UnionWith(assets);
                        }
                    }
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
