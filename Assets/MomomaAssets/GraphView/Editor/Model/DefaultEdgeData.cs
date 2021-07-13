using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;

    [Serializable]
    public class DefaultEdgeData : IEdgeData
    {
        public DefaultEdgeData(string input, string output)
        {
            m_InputPortGuid = input;
            m_OutputPortGuid = output;
        }

        [SerializeField]
        string m_InputPortGuid;
        [SerializeField]
        string m_OutputPortGuid;

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultEdgeDataEditor();
        public string InputPortGuid { get => m_InputPortGuid; set => m_InputPortGuid = value; }
        public string OutputPortGuid { get => m_OutputPortGuid; set => m_OutputPortGuid = value; }

        public GraphElement Deserialize() => new DefaultEdge(this);

        public void DeserializeOverwrite(GraphElement graphElement, GraphView graphView)
        {
            if (!(graphElement is Edge edge))
                throw new InvalidOperationException();
            var inputPort = graphView.GetPortByGuid(InputPortGuid);
            if (edge.input != inputPort)
            {
                edge.input?.Disconnect(edge);
                edge.input = inputPort;
                edge.input.Connect(edge);
            }
            var outputPort = graphView.GetPortByGuid(OutputPortGuid);
            if (edge.output != outputPort)
            {
                edge.output?.Disconnect(edge);
                edge.output = outputPort;
                edge.output.Connect(edge);
            }
            if (edge.output == null || edge.input == null)
            {
                graphView.RemoveElement(edge);
            }
        }

        sealed class DefaultEdgeDataEditor : IGraphElementEditor
        {
            public bool UseDefaultVisualElement => false;

            public void OnDestroy() { }

            public void OnGUI(SerializedProperty property)
            {
                EditorGUILayout.LabelField("Input", property.FindPropertyRelative(nameof(m_InputPortGuid)).stringValue);
                EditorGUILayout.LabelField("Output", property.FindPropertyRelative(nameof(m_OutputPortGuid)).stringValue);
            }
        }

    }
}
