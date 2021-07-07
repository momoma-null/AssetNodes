using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    sealed class CoreAssetProcessor : AssetPostprocessor
    {
        static HashSet<GraphViewObject>? s_GraphViewObjects;

        public static NodeGraphProcessor s_NodeGraphProcessor = new NodeGraphProcessor(AssetDatabase.StartAssetEditing, () =>
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            });

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Length == 0 && deletedAssets.Length == 0 && movedAssets.Length == 0)
                return;
            if (s_GraphViewObjects == null)
            {
                s_GraphViewObjects = new HashSet<GraphViewObject>(AssetDatabase.FindAssets("t:GraphViewObject").
                                        Select(i => AssetDatabase.LoadAssetAtPath<GraphViewObject>(AssetDatabase.GUIDToAssetPath(i))).
                                        Where(i => i != null && i.GraphViewType == typeof(AssetProcessorGUI)));
            }
            else
            {
                s_GraphViewObjects.RemoveWhere(i => i == null);
                foreach (var i in importedAssets)
                {
                    if (AssetDatabase.LoadMainAssetAtPath(i) is GraphViewObject graphViewObject)
                        s_GraphViewObjects.Add(graphViewObject);
                }
            }
            foreach (var i in s_GraphViewObjects)
            {
                s_NodeGraphProcessor.StartProcess(i);
            }
        }
    }
}
