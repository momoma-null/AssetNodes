using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

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
                EditorGUILayout.PropertyField(dataProperty, true);
            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(IGraphElementData), true)]
    sealed class LoadAssetsNodeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 0f;
            using (var endProperty = property.GetEndProperty())
            {
                if (property.NextVisible(true) && !SerializedProperty.EqualContents(property, endProperty))
                {
                    while (true)
                    {
                        height += EditorGUI.GetPropertyHeight(property, true);
                        if (!property.NextVisible(false) || SerializedProperty.EqualContents(property, endProperty))
                            break;
                        height += EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }
            return height;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var conatiner = new VisualElement();
            using (var endProperty = property.GetEndProperty(false))
            using (var copyProperty = property.Copy())
            {
                if (copyProperty.NextVisible(true) && !SerializedProperty.EqualContents(copyProperty, endProperty))
                {
                    while (true)
                    {
                        var field = new PropertyField(copyProperty.Copy());
                        conatiner.Add(field);
                        if (!copyProperty.NextVisible(false) || SerializedProperty.EqualContents(copyProperty, endProperty))
                            break;
                    }
                }
            }
            return conatiner;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (var endProperty = property.GetEndProperty(false))
            {
                if (property.NextVisible(true) && !SerializedProperty.EqualContents(property, endProperty))
                {
                    while (true)
                    {
                        position.height = EditorGUI.GetPropertyHeight(property);
                        EditorGUI.PropertyField(position, property, true);
                        if (!property.NextVisible(false) || SerializedProperty.EqualContents(property, endProperty))
                            break;
                        position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }
        }
    }
}
