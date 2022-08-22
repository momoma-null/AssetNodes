using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Load/From Directory")]
    sealed class LoadAssetFromDirectoryNode : INodeProcessor
    {
        enum ReloadMode
        {
            None,
            AutoReloadImported,
            AutoReloadAll
        }

        LoadAssetFromDirectoryNode() { }

        [SerializeField]
        ReloadMode m_AutoReload;
        [SerializeField]
        bool m_UseAsTest = false;
        [SerializeField]
        DefaultAsset? m_Folder;

        public Color HeaderColor => ColorDefinition.IONode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = new AssetGroup();
            if (!(CoreAssetProcessor.IsProcessing && m_AutoReload == ReloadMode.None))
            {
                if (m_Folder != null)
                {
                    var folderPath = AssetDatabase.GetAssetPath(m_Folder);
                    if (CoreAssetProcessor.IsProcessing && m_AutoReload == ReloadMode.AutoReloadImported)
                    {
                        foreach (var path in CoreAssetProcessor.ImportedAssetsPaths)
                        {
                            if (path.StartsWith(folderPath))
                                assetGroup.Add(new AssetData(path));
                        }
                    }
                    else
                    {
                        if ((!CoreAssetProcessor.IsProcessing && (!CoreAssetProcessor.IsTesting || m_UseAsTest))
                          || (CoreAssetProcessor.IsProcessing && m_AutoReload == ReloadMode.AutoReloadAll && CoreAssetProcessor.ImportedAssetsPaths.Any(path => path.StartsWith(folderPath))))
                        {
                            var guids = AssetDatabase.FindAssets("", new[] { folderPath });
                            assetGroup.UnionWith(guids.Select(i => new AssetData(AssetDatabase.GUIDToAssetPath(i))));
                        }
                    }
                }
            }
            container.SetOutput(0, assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
