using System;
using System.IO;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Importer/Extract Material")]
    sealed class ExtractMaterialNode : INodeProcessor
    {
        ExtractMaterialNode() { }

        public Color HeaderColor => ColorDefinition.ImporterNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default, "Model");
            portDataContainer.AddInputPort(PathDataPortDefinition.Default, "Export Directory");
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, "Model");
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, "Materials");
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            var pathData = container.GetInput(1, PathDataPortDefinition.Default);
            var materials = new AssetGroup();
            container.SetOutput(0, assetGroup);
            container.SetOutput(1, materials);
            if (assetGroup.Count == 0)
                return;
            using (new AssetModificationScope())
            {
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
                            materials.Add(new AssetData(dstPath));
                            isDirty = true;
                        }
                        if (isDirty)
                        {
                            AssetDatabase.WriteImportSettingsIfDirty(assets.AssetPath);
                            assets.Importer.SaveAndReimport();
                        }
                    }
                }
            }
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
