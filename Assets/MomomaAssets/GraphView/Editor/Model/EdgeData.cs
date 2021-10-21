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
    class EdgeData : IEdgeData
    {
        [SerializeField]
        string m_InputPortGuid;
        [SerializeField]
        string m_OutputPortGuid;

        public string GraphElementName => "Edge";
        public int Priority => 1;
        public string InputPortGuid { get => m_InputPortGuid; set => m_InputPortGuid = value; }
        public string OutputPortGuid { get => m_OutputPortGuid; set => m_OutputPortGuid = value; }

        public EdgeData(string input, string output)
        {
            m_InputPortGuid = input;
            m_OutputPortGuid = output;
        }

        public GraphElement Deserialize() => new BindableEdge(this);

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
                edge.input?.Connect(edge);
            }
            var outputPort = graphView.GetPortByGuid(OutputPortGuid);
            if (edge.output != outputPort)
            {
                edge.output?.Disconnect(edge);
                edge.output = outputPort;
                edge.output?.Connect(edge);
            }
            if (edge.output == null || edge.input == null)
            {
                graphView.DeleteElements(new[] { edge });
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

        sealed class EdgeDataEditor : BaseGraphElementEditor
        {
            [GraphElementEditorFactory]
            static void Entry(IEntryDelegate<GenerateGraphElementEditor> factories)
            {
                factories.Add(typeof(EdgeData), (data, property) => new EdgeDataEditor(property));
            }

            readonly SerializedProperty _InputPortProperty;
            readonly SerializedProperty _OutputPortProperty;

            EdgeDataEditor(SerializedProperty property)
            {
                _InputPortProperty = property.FindPropertyRelative(nameof(m_InputPortGuid));
                _OutputPortProperty = property.FindPropertyRelative(nameof(m_OutputPortGuid));
            }

            public override void OnGUI()
            {
                EditorGUILayout.LabelField("Input", _InputPortProperty?.stringValue);
                EditorGUILayout.LabelField("Output", _OutputPortProperty?.stringValue);
            }
        }
    }
}
