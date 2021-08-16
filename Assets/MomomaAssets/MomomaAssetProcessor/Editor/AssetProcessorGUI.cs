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
            if (m_NodeGraph == null)
                m_NodeGraph = new NodeGraph(this, CoreAssetProcessor.s_NodeGraphProcessor);
        }

        void OnDestroy()
        {
            m_NodeGraph?.Dispose();
            m_NodeGraph = null;
        }
    }
}
