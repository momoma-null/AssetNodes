using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Group/Combine")]
    sealed class CombineNode : INodeProcessor
    {
        sealed class CombineNodeEditor : IGraphElementEditor
        {
            public bool UseDefaultVisualElement => false;
            public void OnDestroy() { }
            public void OnGUI(SerializedProperty property)
            {
                using (var m_InputPortsProperty = property.FindPropertyRelative(nameof(m_InputPorts)))
                using (var m_PortTypeProperty = property.FindPropertyRelative($"{nameof(m_OutputPort)}.m_PortType"))
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
                        using (var guidProperty = element.FindPropertyRelative("m_Id"))
                        {
                            guidProperty.stringValue = PortData.GetNewId();
                        }
                    }
                    EditorGUI.BeginChangeCheck();
                    var newValue = UnityObjectTypeUtility.AssetTypePopup(m_PortTypeProperty.stringValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_PortTypeProperty.stringValue = newValue;
                        for (var i = 0; i < m_InputPortsProperty.arraySize; ++i)
                        {
                            using (var element = m_InputPortsProperty.GetArrayElementAtIndex(i))
                            using (var portTypeProperty = element.FindPropertyRelative("m_PortType"))
                                portTypeProperty.stringValue = newValue;
                        }
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
        public IEnumerable<PortData> InputPorts => m_InputPorts;
        public IEnumerable<PortData> OutputPorts => new[] { m_OutputPort };

        [SerializeField]
        [HideInInspector]
        PortData[] m_InputPorts = new[] { new PortData(typeof(UnityObject)) };
        [SerializeField]
        [HideInInspector]
        PortData m_OutputPort = new PortData(typeof(UnityObject));

        [SerializeField]
        string m_TypeName = "";

        public void Process(ProcessingDataContainer container)
        {
            var output = new AssetGroup();
            foreach (var i in m_InputPorts)
                output.UnionWith(container.Get(i, this.NewAssetGroup));
            container.Set(m_OutputPort, output);
        }
    }
}
