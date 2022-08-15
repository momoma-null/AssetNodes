using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    internal sealed class CoreAssetProcessor : AssetPostprocessor
    {
        public static bool IsProcessing { get; private set; }
        public static bool IsTesting { get; internal set; }
        public static IEnumerable<string> ImportedAssetsPaths { get; private set; } = Array.Empty<string>();

        public static NodeGraphProcessor s_NodeGraphProcessor = new NodeGraphProcessor(AssetDatabase.StartAssetEditing,
            () =>
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            },
            () =>
            {
                Resources.UnloadUnusedAssets();
            });

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Length == 0 || IsProcessing)
                return;
            try
            {
                IsProcessing = true;
                var paths = new HashSet<string>(importedAssets);
                ImportedAssetsPaths = paths;
                foreach (var i in GraphViewObject.GetGraphViewObjects<AssetProcessorGUI>())
                {
                    s_NodeGraphProcessor.StartProcess(i);
                }
            }
            finally
            {
                IsProcessing = false;
                ImportedAssetsPaths = Array.Empty<string>();
            }
        }
    }
}
