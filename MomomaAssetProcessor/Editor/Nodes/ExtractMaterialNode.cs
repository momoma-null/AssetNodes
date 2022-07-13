using System;
using System.IO;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Importer/Extract Material")]
    sealed class ExtractMaterialNode : INodeProcessor
    {
        ExtractMaterialNode() { }

        [SerializeField]
        string m_DirectoryPath = "../Materials";

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
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

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
