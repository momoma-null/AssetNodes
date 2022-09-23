using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

//#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    static class PrefabModifierUtility
    {
        public static void ModifyPrefab(this IPrefabModifier modifier, AssetGroup assetGroup)
        {
            var regex = new Regex(modifier.RegexPattern);
            using (new AssetModificationScope())
            {
                foreach (var assets in assetGroup)
                {
                    if (!(assets.MainAssetType == typeof(GameObject)) || (assets.MainAsset.hideFlags & HideFlags.NotEditable) != 0)
                        continue;
                    var root = PrefabUtility.LoadPrefabContents(assets.AssetPath);
                    try
                    {
                        if (modifier.IncludeChildren)
                            modifier.ModifyRecursively(root.transform, regex);
                        else if (regex.IsMatch(root.name))
                            modifier.Modify(root);
                    }
                    finally
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, assets.AssetPath);
                        PrefabUtility.UnloadPrefabContents(root);
                    }
                }
            }
        }

        static void ModifyRecursively(this IPrefabModifier modifier, Transform transform, Regex regex)
        {
            if (regex.IsMatch(transform.name))
                modifier.Modify(transform.gameObject);
            foreach (Transform child in transform)
                modifier.ModifyRecursively(child, regex);
        }
    }

    interface IPrefabModifier
    {
        bool IncludeChildren { get; }
        string RegexPattern { get; }
        void Modify(GameObject go);
    }
}
