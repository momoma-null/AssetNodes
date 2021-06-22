using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    sealed class AssetProcessorGUI : EditorWindow
    {
        [MenuItem("MomomaTools/Asset Processor", false, 500)]
        static void ShowWindow()
        {
            EditorWindow.GetWindow<AssetProcessorGUI>("MomomaAssetProcessor");
        }

        NodeGraph? m_NodeGraph;

        void OnEnable()
        {
            m_NodeGraph = new NodeGraph(this);
            m_NodeGraph.PostProcess += AssetDatabase.StartAssetEditing;
            m_NodeGraph.PostProcess += () =>
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            };
        }

        void OnDisable()
        {
            m_NodeGraph?.Dispose();
            m_NodeGraph = null;
        }
    }
}
