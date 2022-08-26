using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    static class PrefabModifierUtility
    {
        public static void ModifyPrefab(this IPrefabModifier modifier, AssetGroup assetGroup)
        {
            var regex = new Regex(modifier.RegexPattern);
            foreach (var assets in assetGroup)
            {
                if (!(assets.MainAssetType == typeof(GameObject)) || (assets.MainAsset.hideFlags & HideFlags.NotEditable) != 0)
                    continue;
                using (var scope = new PrefabUtility.EditPrefabContentsScope(assets.AssetPath))
                {
                    var root = scope.prefabContentsRoot;
                    if (modifier.IncludeChildren)
                        modifier.ModifyRecursively(root.transform, regex);
                    else if (regex.IsMatch(root.name))
                        modifier.Modify(root);
                }
                AssetDatabase.ImportAsset(assets.AssetPath);
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
