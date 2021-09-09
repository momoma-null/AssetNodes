using UnityEditor;

//#nullable enable

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
        readonly DefaultGraphElementEditor m_DefaultGraphElementEditor = new DefaultGraphElementEditor();

        public bool UseDefaultVisualElement => m_DefaultGraphElementEditor.UseDefaultVisualElement;

        public void OnEnable() => m_DefaultGraphElementEditor.OnEnable();
        public void OnDisable() => m_DefaultGraphElementEditor.OnDisable();
        public void OnGUI(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty)
                    => m_DefaultGraphElementEditor.OnGUI(processorProperty);
    }
}
