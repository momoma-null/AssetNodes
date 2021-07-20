using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    [CustomEditor(typeof(GraphElementObject))]
    sealed class GraphElementObjectInspector : Editor
    {
        SerializedProperty? m_GraphElementDataProperty;

        void OnEnable()
        {
            if (target != null)
                m_GraphElementDataProperty = serializedObject.FindProperty("m_GraphElementData");
        }

        void OnDisable()
        {
            m_GraphElementDataProperty = null;
        }

        void OnDestroy()
        {
            if (target is GraphElementObject graphElementObject)
            {
                var editor = graphElementObject.GraphElementData?.GraphElementEditor;
                editor?.OnDestroy();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            if (target is GraphElementObject graphElementObject)
            {
                var editor = graphElementObject.GraphElementData?.GraphElementEditor;
                if (editor != null && m_GraphElementDataProperty != null)
                    editor.OnGUI(m_GraphElementDataProperty);
            }
            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }
    }
}
