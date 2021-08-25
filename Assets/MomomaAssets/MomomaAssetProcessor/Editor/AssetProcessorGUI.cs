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

        [SerializeField]
        NodeGraphEditorData? m_Data;

        NodeGraph? m_NodeGraph;

        void OnEnable()
        {
            if (m_Data == null)
                m_Data = new NodeGraphEditorData();
            if (m_NodeGraph == null)
                m_NodeGraph = new NodeGraph(this, CoreAssetProcessor.s_NodeGraphProcessor, m_Data);
        }

        void OnDisable()
        {
            m_Data?.OnDisable(Unsupported.IsDestroyScriptableObject(this));
            m_NodeGraph?.Dispose();
            m_NodeGraph = null;
        }
    }
}
