using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    [CustomEditor(typeof(GraphElementObject))]
    sealed class GraphElementObjectInspector : Editor
    {
        SerializedProperty? m_GraphElementData;

        void OnEnable()
        {
            m_GraphElementData = serializedObject.FindProperty(nameof(m_GraphElementData));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_GraphElementData, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
