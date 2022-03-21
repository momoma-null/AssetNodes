using UnityEditor;

//#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class SerializedNodeProcessor
    {
        readonly SerializedProperty _ProcessorProperty;
        readonly SerializedPropertyList _InputPorts;
        readonly SerializedPropertyList _OutputPorts;

        public SerializedPropertyList InputPorts => _InputPorts;
        public SerializedPropertyList OutputPorts => _OutputPorts;

        internal SerializedNodeProcessor(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty)
        {
            _ProcessorProperty = processorProperty;
            _InputPorts = new SerializedPropertyList(inputPortsProperty);
            _OutputPorts = new SerializedPropertyList(outputPortsProperty);
        }

        public SerializedProperty GetProcessorProperty() => _ProcessorProperty.Copy();
    }
}
