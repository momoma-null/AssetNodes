using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    [CustomEditor(typeof(GraphElementObject))]
    sealed class GraphElementObjectInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (var dataProperty = serializedObject.FindProperty("m_GraphElementData"))
            using (var endProperty = dataProperty.GetEndProperty(false))
            {
                dataProperty.NextVisible(true);
                while (true)
                {
                    if (SerializedProperty.EqualContents(endProperty, dataProperty))
                        break;
                    EditorGUILayout.PropertyField(dataProperty.Copy(), true);
                    if (!dataProperty.NextVisible(false))
                        break;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
