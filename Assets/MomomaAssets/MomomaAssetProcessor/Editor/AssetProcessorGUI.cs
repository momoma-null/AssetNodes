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

        NodeGraph<AssetProcessorGraph, AdvancedEdge>? m_NodeGraph;

        void OnEnable()
        {
            m_NodeGraph = new NodeGraph<AssetProcessorGraph, AdvancedEdge>(this, new AssetProcessorGraph());
        }

        void OnDisable()
        {
            m_NodeGraph?.Dispose();
            m_NodeGraph = null;
        }
    }
}
