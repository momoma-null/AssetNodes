using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [ScriptedImporter(1, "readonlyprefab")]
    sealed class ReadonlyPrefab : ScriptedImporter
    {
        [SerializeField]
        GameObject? m_SourcePrefab = null;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (m_SourcePrefab == null)
                return;
            var root = Instantiate(m_SourcePrefab);
            root.name = m_SourcePrefab.name;
            ctx.AddObjectToAsset("root", root);
            ctx.SetMainObject(root);
            var srcPath = AssetDatabase.GetAssetPath(m_SourcePrefab);
            if (!string.IsNullOrEmpty(srcPath))
                ctx.DependsOnSourceAsset(srcPath);
        }
    }
}
