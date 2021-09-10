using System;
using System.IO;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Importer/Extract Material")]
    sealed class ExtractMaterialNode : INodeProcessor
    {
        static ExtractMaterialNode()
        {
            INodeDataUtility.AddConstructor(() => new ExtractMaterialNode());
        }

        ExtractMaterialNode() { }

        [SerializeField]
        string m_DirectoryPath = "../Materials";

        public INodeProcessorEditor ProcessorEditor { get; } = new DefaultNodeProcessorEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<GameObject>(isMulti: true);
            portDataContainer.AddOutputPort<GameObject>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            foreach (var assets in assetGroup)
            {
                var path = assets.AssetPath;
                if (AssetImporter.GetAtPath(path) is ModelImporter)
                {
                    var directoryPath = Path.Combine(Path.GetDirectoryName(path), m_DirectoryPath);
                    var isDirty = false;
                    foreach (var i in AssetDatabase.LoadAllAssetsAtPath(path))
                    {
                        if (!(i is Material))
                            continue;
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                            AssetDatabase.ImportAsset(directoryPath);
                        }
                        var dstPath = Path.Combine(directoryPath, $"{i.name}.mat");
                        AssetDatabase.ExtractAsset(i, dstPath);
                        isDirty = true;
                    }
                    if (isDirty)
                    {
                        AssetDatabase.WriteImportSettingsIfDirty(path);
                        AssetDatabase.ImportAsset(path);
                    }
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
