using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Object;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Generate/Prefab Variant")]
    sealed class GeneratePrefabVariantNode : INodeProcessor
    {
        static GeneratePrefabVariantNode()
        {
            INodeDataUtility.AddConstructor(() => new GeneratePrefabVariantNode());
        }

        GeneratePrefabVariantNode() { }

        [SerializeField]
        string m_OriginalPrefabPath = "Assets/(.+).prefab";
        [SerializeField]
        string m_VariantPrefabPath = "Assets/$1_Variant.prefab";

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<GameObject>(isMulti: true);
            portDataContainer.AddOutputPort<GameObject>(isMulti: true);
            portDataContainer.AddOutputPort<GameObject>("Variant", true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            var variants = new AssetGroup();
            var regex = new Regex(m_OriginalPrefabPath);
            foreach (var assets in assetGroup)
            {
                if (assets.MainAsset is GameObject prefab)
                {
                    var srcPath = assets.AssetPath;
                    var dstPath = regex.Replace(srcPath, m_VariantPrefabPath);
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
    }
}
