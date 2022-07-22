using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class SerializedNodeProcessor
    {
        readonly SerializedProperty _ProcessorProperty;

        internal SerializedNodeProcessor(SerializedProperty processorProperty)
        {
            _ProcessorProperty = processorProperty;
        }

        public SerializedProperty GetProcessorProperty() => _ProcessorProperty.Copy();
    }
}
