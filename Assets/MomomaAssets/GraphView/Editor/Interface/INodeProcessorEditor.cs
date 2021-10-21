using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface INodeProcessorEditor
    {
        bool UseDefaultVisualElement { get; }
        void OnEnable();
        void OnDisable();
        void OnGUI(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty);
    }

    public class DefaultNodeProcessorEditor : INodeProcessorEditor
    {
        public bool UseDefaultVisualElement => true;

        public void OnEnable() { }
        public void OnDisable() { }
        public void OnGUI(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty)
        {
            using (var property = processorProperty.Copy())
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
