using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    [CustomEditor(typeof(GraphElementObject))]
    sealed class GraphElementObjectInspector : Editor
    {
        [SerializeReference]
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
            m_Editor?.OnDisable(Unsupported.IsDestroyScriptableObject(this));
            m_Editor = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            if (target is GraphElementObject graphElementObject)
            {
                var editor = graphElementObject.GraphElementData?.GraphElementEditor;
                if (editor != null && m_GraphElementDataProperty != null)
                    using (var prop = m_GraphElementDataProperty.Copy())
                        editor.OnGUI(prop);
            }
            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }
    }
}
