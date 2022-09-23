using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    sealed class ComponentPathAttribute : PropertyAttribute
    {
        public ComponentPathAttribute() { }
    }

    [CustomPropertyDrawer(typeof(ComponentPathAttribute))]
    sealed class ComponentPathProperty : PropertyDrawer
    {
        const string LABEL_NAME = "Component Type";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return UnityObjectTypeUtility.CreatePropertyGUI(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            label.text = LABEL_NAME;
            var newValue = UnityObjectTypeUtility.ComponentTypePopup(position, label, property.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = newValue;
            }
        }
    }
}
