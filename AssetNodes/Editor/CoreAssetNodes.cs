using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    internal sealed class CoreAssetNodes : AssetPostprocessor
    {
        public static bool IsProcessing { get; private set; }
        public static bool IsTesting { get; internal set; }
        public static IEnumerable<string> ImportedAssetsPaths { get; private set; } = Array.Empty<string>();
        public static NodeGraphProcessor Processor { get; } = new NodeGraphProcessor(OnCompleteProcess);

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            EditorApplication.delayCall += () =>
            {
                var validAssets = Array.FindAll(importedAssets, path => path.StartsWith("Assets/"));
                if (validAssets.Length == 0 || IsProcessing)
                    return;
                try
                {
                    IsProcessing = true;
                    ImportedAssetsPaths = validAssets;
                    foreach (var i in GraphViewObject.GetGraphViewObjects<AssetNodesGUI>())
                    {
                        Processor.StartProcess(i);
                    }
                }
                finally
                {
                    IsProcessing = false;
                    ImportedAssetsPaths = Array.Empty<string>();
                }
            };
        }

        static void OnCompleteProcess()
        {
            AssetDatabase.SaveAssets();
            Resources.UnloadUnusedAssets();
        }
    }
}
