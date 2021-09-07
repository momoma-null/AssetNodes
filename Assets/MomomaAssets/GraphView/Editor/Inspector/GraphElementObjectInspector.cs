using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    [CustomEditor(typeof(GraphElementObject))]
    [DefaultExecutionOrder(-9)]
    sealed class GraphElementObjectInspector : Editor
    {
        IGraphElementEditor? m_Editor;
        SerializedProperty? m_GraphElementDataProperty;

        void OnEnable()
        {
            m_GraphElementDataProperty = serializedObject.FindProperty("m_GraphElementData");
            if (target is GraphElementObject graphElementObject)
            {
                m_Editor = graphElementObject.GraphElementData?.GraphElementEditor;
            }
            m_Editor?.OnEnable();
        }

        void OnDisable()
        {
            m_GraphElementDataProperty?.Dispose();
            m_GraphElementDataProperty = null;
            m_Editor?.OnDisable();
            m_Editor = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            if (target is GraphElementObject graphElementObject)
            {
                if (m_Editor != null && m_GraphElementDataProperty != null)
                {
                    using (var prop = m_GraphElementDataProperty.Copy())
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        m_Editor.OnGUI(prop);
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
}
