using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    public sealed class AssetGroup : HashSet<AssetData>, IProcessingData
    {
        public static Func<IEnumerable<AssetGroup>, AssetGroup> combineAssetGroup = assetGroups =>
        {
            var combined = new AssetGroup();
            foreach (var i in assetGroups)
                combined.UnionWith(i);
            return combined;
        };

        public AssetGroup() { }
        public AssetGroup(IEnumerable<AssetData> collection) : base(collection) { }
    }

    public sealed class AssetGroupPortDefinition : IPortDefinition<AssetGroup>
    {
        public static AssetGroupPortDefinition Default { get; } = new AssetGroupPortDefinition();

        AssetGroupPortDefinition() { }

        public bool IsMultiInput => true;
        public bool IsMultiOutput => true;
        public Color PortColor { get; } = new Color(0.85f, 0.85f, 0.85f, 1f);

        public AssetGroup CombineInputData(IEnumerable<AssetGroup> inputs)
        {
            var combined = new AssetGroup();
            foreach (var i in inputs)
                combined.UnionWith(i);
            return combined;
        }
    }

    public sealed class AssetData
    {
        Type? m_MainAssetType;
        UnityObject? m_MainAsset;
        UnityObject[]? m_AllAssets;
        AssetImporter? m_Importer;

        public Type MainAssetType => m_MainAssetType ?? (m_MainAssetType = AssetDatabase.GetMainAssetTypeAtPath(AssetPath));
        public UnityObject MainAsset => m_MainAsset ?? (m_MainAsset = AssetDatabase.LoadMainAssetAtPath(AssetPath));
        public IEnumerable<UnityObject> AllAssets => m_AllAssets ?? (m_AllAssets = (Path.GetExtension(AssetPath) == ".unity") ? new UnityObject[] { MainAsset } : AssetDatabase.LoadAllAssetsAtPath(AssetPath));
        public string AssetPath { get; }
        public AssetImporter? Importer => m_Importer ?? (m_Importer = AssetImporter.GetAtPath(AssetPath));

        public AssetData(string path)
        {
            AssetPath = path;
        }

        public IEnumerable<T> GetAssetsFromType<T>() where T : UnityObject
        {
            foreach (var i in AllAssets)
                if (i is T tObj)
                    yield return tObj;
        }
    }
}
