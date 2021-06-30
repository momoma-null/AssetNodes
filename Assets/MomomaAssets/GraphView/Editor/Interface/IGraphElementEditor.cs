using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IGraphElementEditor
    {
        bool UseDefaultVisualElement { get; }
        void OnDestroy();
        void OnGUI(SerializedProperty property);
    }

    public sealed class DefaultGraphElementEditor : IGraphElementEditor
    {
        public bool UseDefaultVisualElement => true;

        public void OnDestroy() { }

        public void OnGUI(SerializedProperty property)
        {
            using (var endProperty = property.GetEndProperty(false))
            {
                if (property.NextVisible(true) && !SerializedProperty.EqualContents(property, endProperty))
                {
                    while (true)
                    {
                        EditorGUILayout.PropertyField(property, true);
                        if (!property.NextVisible(false) || SerializedProperty.EqualContents(property, endProperty))
                            break;
                    }
                }
            }
        }
    }
}
