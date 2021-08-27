using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IGraphElementEditor
    {
        bool UseDefaultVisualElement { get; }
        void OnEnable();
        void OnDisable();
        void OnGUI(SerializedProperty property);
    }

    public sealed class DefaultGraphElementEditor : IGraphElementEditor
    {
        public bool UseDefaultVisualElement => true;

        public void OnEnable() { }
        public void OnDisable() { }

        public void OnGUI(SerializedProperty property)
        {
            using (var endProperty = property.GetEndProperty(false))
            {
                if (property.NextVisible(true))
                {
                    while (true)
                    {
                        if (SerializedProperty.EqualContents(property, endProperty))
                            break;
                        EditorGUILayout.PropertyField(property, true);
                        if (!property.NextVisible(false))
                            break;
                    }
                }
            }
        }
    }
}
