using UnityEngine;
using UnityEditor;

//#nullable enable

namespace MomomaAssets.GraphView
{
    [CustomEditor(typeof(GraphViewObject))]
    sealed class GraphViwObjectInspector : Editor
    {
        static class Styles
        {
            static public GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            static public GUIContent graphType = EditorGUIUtility.TrTextContent("Graph Type");
        }

        public override void OnInspectorGUI()
        {
            if (target is GraphViewObject graphViewObject)
            {
                EditorGUILayout.LabelField(Styles.graphType, new GUIContent(graphViewObject.GraphViewType?.FullName), Styles.labelStyle);
            }
        }
    }
}
