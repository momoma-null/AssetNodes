using System;
using System.Collections.Generic;
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
        [SerializeField]
        string m_InputPortGuid;
        [SerializeField]
        string m_OutputPortGuid;

        DefaultEdgeDataEditor? m_Editor;

        public int Priority => 1;
        public IGraphElementEditor GraphElementEditor => m_Editor ?? (m_Editor = new DefaultEdgeDataEditor());
        public string InputPortGuid { get => m_InputPortGuid; set => m_InputPortGuid = value; }
        public string OutputPortGuid { get => m_OutputPortGuid; set => m_OutputPortGuid = value; }

        public DefaultEdgeData(string input, string output)
        {
            m_InputPortGuid = input;
            m_OutputPortGuid = output;
        }

        public GraphElement Deserialize() => new DefaultEdge(this);

        public void SetPosition(GraphElement graphElement, Rect position) => graphElement.SetPosition(position);

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

        public void ReplaceGuid(Dictionary<string, string> guids)
        {
            if (!guids.TryGetValue(m_InputPortGuid, out var newGuid))
            {
                newGuid = PortData.GetNewId();
                guids.Add(m_InputPortGuid, newGuid);
            }
            m_InputPortGuid = newGuid;
            if (!guids.TryGetValue(m_OutputPortGuid, out newGuid))
            {
                newGuid = PortData.GetNewId();
                guids.Add(m_OutputPortGuid, newGuid);
            }
            m_OutputPortGuid = newGuid;
        }

        sealed class DefaultEdgeDataEditor : IGraphElementEditor
        {
            public bool UseDefaultVisualElement => false;

            public void OnEnable() { }
            public void OnDisable(bool isDestroying) { }

            public void OnGUI(SerializedProperty property)
            {
                EditorGUILayout.LabelField("Input", property.FindPropertyRelative(nameof(m_InputPortGuid)).stringValue);
                EditorGUILayout.LabelField("Output", property.FindPropertyRelative(nameof(m_OutputPortGuid)).stringValue);
            }
        }

    }
}
