using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Object;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Generate/Prefab Variant")]
    sealed class GeneratePrefabVariantNode : INodeProcessor
    {
        GeneratePrefabVariantNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, "Original");
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, "Variant");
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroupPortDefinition.Default);
            var pathData = container.Get(portDataContainer.InputPorts[1], PathDataPortDefinition.Default);
            var variants = new AssetGroup();
            foreach (var assets in assetGroup)
            {
                if (assets.MainAsset is GameObject prefab)
                {
                    var dstPath = pathData.GetPath(assets);
                    var directoryPath = Path.GetDirectoryName(dstPath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                        AssetDatabase.ImportAsset(directoryPath);
                    }
                    var currentDstPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dstPath);
                    if (currentDstPrefab == null || PrefabUtility.GetPrefabAssetType(currentDstPrefab) != PrefabAssetType.Variant || PrefabUtility.GetCorrespondingObjectFromOriginalSource(currentDstPrefab) != prefab)
                    {
                        var instance = PrefabUtility.InstantiatePrefab(prefab);
                        try
                        {
                            currentDstPrefab = PrefabUtility.SaveAsPrefabAsset(instance as GameObject, dstPath);
                        }
                        finally
                        {
                            DestroyImmediate(instance);
                        }
                    }
                    variants.Add(new AssetData(dstPath));
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
            container.Set(portDataContainer.OutputPorts[1], variants);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
