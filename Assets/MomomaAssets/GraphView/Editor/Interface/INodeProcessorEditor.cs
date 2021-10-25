using UnityEditor;

//#nullable enable

namespace MomomaAssets.GraphView
{
    public interface INodeProcessorEditor
    {
        bool UseDefaultVisualElement { get; }
        void OnEnable();
        void OnDisable();
        void OnGUI();
    }

    class DefaultNodeProcessorEditor : INodeProcessorEditor
    {
        readonly SerializedProperty _ProcessorProperty;

        public bool UseDefaultVisualElement => true;

        public DefaultNodeProcessorEditor(SerializedProperty processorProperty)
        {
            _ProcessorProperty = processorProperty;
        }

        public void OnEnable() { }
        public void OnDisable() { }
        public void OnGUI()
        {
            using (var property = _ProcessorProperty.Copy())
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
