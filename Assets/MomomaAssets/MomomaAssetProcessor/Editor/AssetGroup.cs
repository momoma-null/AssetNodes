using System.Collections.Generic;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    public sealed class AssetGroup : HashSet<AssetData>, IProcessingData
    {
        public AssetGroup() { }
        public AssetGroup(IEnumerable<AssetData> source) : base(source) { }

        public string GropuName { get; set; } = "";
    }

    public static class AssetGroupExtension
    {
        public static AssetGroup NewAssetGroup(this INodeProcessor t) => new AssetGroup();
    }

    public sealed class AssetData
    {
        public UnityObject MainAsset { get; }
        public UnityObject[] AllAssets { get; }
        public string AssetPath { get; }
        public AssetImporter? Importer { get; }

        public AssetData(string path)
        {
            MainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            if (MainAsset is SceneAsset)
                AllAssets = new UnityObject[0];
            else
                AllAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            AssetPath = path;
            Importer = AssetImporter.GetAtPath(path);
        }

        public IEnumerable<T> GetAssetsFromType<T>() where T : UnityObject
        {
            foreach (var i in AllAssets)
                if (i is T tObj)
                    yield return tObj;
        }
    }
}
