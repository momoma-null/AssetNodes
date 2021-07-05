using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    sealed class CoreAssetProcessor : AssetPostprocessor
    {
        static HashSet<GraphViewObject> s_GraphViewObjects;
        static bool s_InProcess = false;

        public static NodeGraphProcessor s_NodeGraphProcessor = new NodeGraphProcessor(AssetDatabase.StartAssetEditing, () =>
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            });

        static CoreAssetProcessor()
        {
            s_GraphViewObjects = new HashSet<GraphViewObject>(AssetDatabase.FindAssets("t:GraphViewObject").
                                        Select(i => AssetDatabase.LoadAssetAtPath<GraphViewObject>(AssetDatabase.GUIDToAssetPath(i))).
                                        Where(i => i != null && i.GraphViewType == typeof(AssetProcessorGUI)));
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (s_InProcess)
                return;
            if (importedAssets.Length == 0 && deletedAssets.Length == 0 && movedAssets.Length == 0)
                return;
            s_GraphViewObjects.RemoveWhere(i => i == null);
            foreach (var i in importedAssets)
            {
                if (AssetDatabase.LoadMainAssetAtPath(i) is GraphViewObject graphViewObject)
                    s_GraphViewObjects.Add(graphViewObject);
            }
            try
            {
                s_InProcess = true;
                foreach (var i in s_GraphViewObjects)
                {
                    s_NodeGraphProcessor.StartProcess(i);
                }
            }
            finally
            {
                s_InProcess = false;
            }
        }
    }
}
