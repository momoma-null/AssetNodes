using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [CreateElement("Group/Combine")]
    sealed class CombineNode : INodeProcessor, ISerializationCallbackReceiver
    {
        sealed class CombineNodeEditor : IGraphElementEditor
        {
            public bool UseDefaultVisualElement => false;
            public void OnDestroy() { }
            public void OnGUI(SerializedProperty property)
            {
                using (var m_InputPortsProperty = property.FindPropertyRelative(nameof(m_InputPortIds)))
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(m_InputPortsProperty.arraySize < 2))
                    {
                        if (GUILayout.Button("-"))
                            --m_InputPortsProperty.arraySize;
                    }
                    if (GUILayout.Button("+"))
                    {
                        ++m_InputPortsProperty.arraySize;
                        using (var element = m_InputPortsProperty.GetArrayElementAtIndex(m_InputPortsProperty.arraySize - 1))
                        {
                            element.stringValue = PortData.GetNewId();
                        }
                    }
                }
                using (var m_TypeIndexProperty = property.FindPropertyRelative(nameof(m_TypeIndex)))
                {
                    EditorGUI.BeginChangeCheck();
                    var newIndex = EditorGUILayout.Popup(m_TypeIndexProperty.intValue, UnityObjectTypeUtility.TypeNames);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_TypeIndexProperty.intValue = newIndex;
                    }
                }
            }
        }

        static CombineNode()
        {
            INodeDataUtility.AddConstructor(() => new CombineNode());
        }

        CombineNode() { }

        public IGraphElementEditor GraphElementEditor { get; } = new CombineNodeEditor();
        public IEnumerable<PortData> InputPorts { get; private set; } = new PortData[0];
        public IEnumerable<PortData> OutputPorts { get; private set; } = new PortData[0];

        [SerializeField]
        [HideInInspector]
        string[] m_InputPortIds = new[] { PortData.GetNewId() };
        [SerializeField]
        [HideInInspector]
        string m_OutputPortId = PortData.GetNewId();

        [SerializeField]
        int m_TypeIndex;

        public void Process(ProcessingDataContainer container)
        {
            var output = new AssetGroup();
            for (var i = 0; i < m_InputPortIds.Length; ++i)
                output.UnionWith(container.Get(m_InputPortIds[i], () => new AssetGroup()));
            container.Set(m_OutputPortId, output);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            var type = UnityObjectTypeUtility.GetAssetTypeData(m_TypeIndex).AssetType;
            var inputPorts = new PortData[m_InputPortIds.Length];
            for (var i = 0; i < inputPorts.Length; ++i)
                inputPorts[i] = new PortData(type, id: m_InputPortIds[i]);
            InputPorts = inputPorts;
            OutputPorts = new[] { new PortData(type, id: m_OutputPortId) };
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
