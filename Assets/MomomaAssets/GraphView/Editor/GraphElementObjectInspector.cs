using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    [CustomEditor(typeof(GraphElementObject))]
    sealed class GraphElementObjectInspector : Editor
    {
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
                if (editor != null)
                {
                    using (var dataProperty = serializedObject.FindProperty("m_GraphElementData"))
                        editor.OnGUI(dataProperty);
                }
            }
            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }
    }
}
