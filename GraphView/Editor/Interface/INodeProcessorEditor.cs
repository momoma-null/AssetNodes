using System;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface INodeProcessorEditor : IDisposable
    {
        bool UseDefaultVisualElement { get; }
        void OnGUI();
    }

    class DefaultNodeProcessorEditor : INodeProcessorEditor
    {
        readonly SerializedProperty _ProcessorProperty;

        public bool UseDefaultVisualElement => true;

        public DefaultNodeProcessorEditor(SerializedNodeProcessor serializedNodeProcessor)
        {
            _ProcessorProperty = serializedNodeProcessor.GetProcessorProperty();
        }

        public void Dispose() { }

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
