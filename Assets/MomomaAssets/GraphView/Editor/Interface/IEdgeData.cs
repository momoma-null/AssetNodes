using System;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IEdgeData : IGraphElementData
    {
        string InputPortGuid { get; set; }
        string OutputPortGuid { get; set; }
    }

    [Serializable]
    public class DefaultEdgeData : IEdgeData
    {
        public DefaultEdgeData(string input, string output)
        {
            m_InputPortGuid = input;
            m_OutputPortGuid = output;
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

        [SerializeField]
        string m_InputPortGuid;
        [SerializeField]
        string m_OutputPortGuid;

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultEdgeDataEditor();
        public string InputPortGuid { get => m_InputPortGuid; set => m_InputPortGuid = value; }
        public string OutputPortGuid { get => m_OutputPortGuid; set => m_OutputPortGuid = value; }
    }
}
