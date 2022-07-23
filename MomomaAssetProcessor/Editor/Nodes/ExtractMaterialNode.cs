using System;
using System.IO;
using UnityEditor;
using UnityEngine;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Importer/Extract Material")]
    sealed class ExtractMaterialNode : INodeProcessor
    {
        ExtractMaterialNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            var pathData = container.GetInput(1, PathDataPortDefinition.Default);
            foreach (var assets in assetGroup)
            {
                if (assets.Importer is ModelImporter)
                {
                    var directoryPath = pathData.GetPath(assets);
                    var isDirty = false;
                    foreach (var i in assets.GetAssetsFromType<Material>())
                    {
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
                        AssetDatabase.WriteImportSettingsIfDirty(assets.AssetPath);
                        assets.Importer.SaveAndReimport();
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
