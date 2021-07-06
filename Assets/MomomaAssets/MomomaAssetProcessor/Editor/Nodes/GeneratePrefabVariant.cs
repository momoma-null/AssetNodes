using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [Serializable]
    sealed class GeneratePrefabVariant : INodeData
    {
        static GeneratePrefabVariant()
        {
            INodeDataUtility.AddConstructor(() => new GeneratePrefabVariant());
        }

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();
        public string MenuPath => "Generate/Prefab Variant";
        public IEnumerable<PortData> InputPorts => new[] { m_InputPort };
        public IEnumerable<PortData> OutputPorts => m_OutputPorts;

        [SerializeField]
        [HideInInspector]
        PortData m_InputPort = new PortData(typeof(GameObject));

        [SerializeField]
        [HideInInspector]
        PortData[] m_OutputPorts = new[] { new PortData(typeof(GameObject)), new PortData(typeof(GameObject), "Variant") };

        [SerializeField]
        string m_OriginalPrefabPath = "Assets/(.+).prefab";

        [SerializeField]
        string m_VariantPrefabPath = "Assets/$1_Variant.prefab";

        public void Process(ProcessingDataContainer container)
        {
            var assetGroup = container.Get(m_InputPort.Id, () => new AssetGroup());
            var variants = new AssetGroup();
            var regex = new Regex(m_OriginalPrefabPath);
            foreach (var asset in assetGroup)
            {
                if (asset is GameObject prefab)
                {
                    var srcPath = AssetDatabase.GetAssetPath(asset);
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
                    variants.Add(currentDstPrefab);
                }
            }
            container.Set(m_OutputPorts[0].Id, assetGroup);
            container.Set(m_OutputPorts[1].Id, variants);
        }
    }
}
