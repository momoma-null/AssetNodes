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
            EditorGUILayout.PropertyField(m_GraphElementData, true);
        }
    }
}
