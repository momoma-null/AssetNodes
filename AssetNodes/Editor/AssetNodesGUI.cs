using UnityEngine;
using UnityEditor;

//#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    internal sealed class AssetNodesGUI : EditorWindow
    {
        [MenuItem("MomomaTools/Asset Nodes", false, 500)]
        static void ShowWindow()
        {
            EditorWindow.GetWindow<AssetNodesGUI>("Asset Nodes");
        }

        [SerializeField]
        NodeGraphEditorData m_Data;

        NodeGraph m_NodeGraph;

        void OnEnable()
        {
            if (m_Data == null)
                m_Data = new NodeGraphEditorData();
            if (m_NodeGraph == null)
                m_NodeGraph = new NodeGraph(this, CoreAssetNodes.s_NodeGraphProcessor, m_Data);
        }

        void OnDisable()
        {
            m_NodeGraph?.Dispose();
            m_NodeGraph = null;
        }
    }
}
