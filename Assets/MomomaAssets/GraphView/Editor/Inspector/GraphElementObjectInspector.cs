using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    [CustomEditor(typeof(GraphElementObject))]
    [DefaultExecutionOrder(-9)]
    sealed class GraphElementObjectInspector : Editor
    {
        BaseGraphElementEditor? m_Editor;

        void OnEnable()
        {
            if (target is GraphElementObject graphElementObject && graphElementObject.GraphElementData != null)
            {
                var graphElementDataProperty = serializedObject.FindProperty("m_GraphElementData");
                m_Editor = GraphElementEditorFactory.CreateEditor(graphElementObject.GraphElementData, graphElementDataProperty);
            }
        }

        void OnDisable()
        {
            m_Editor?.Dispose();
            m_Editor = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            if (m_Editor != null)
            {
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    m_Editor.OnGUI();
                    if (change.changed)
                    {
                        serializedObject.ApplyModifiedProperties();
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }
                }
            }
        }
    }
}
