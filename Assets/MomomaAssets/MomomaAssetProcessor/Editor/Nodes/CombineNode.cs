using System;
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
        static CombineNode()
        {
            INodeDataUtility.AddConstructor(() => new CombineNode());
        }

        CombineNode() { }

        [SerializeField]
        string m_Type = "";

        public INodeProcessorEditor ProcessorEditor { get; } = new CombineNodeEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(UnityObject), isMulti: true));
            portDataContainer.OutputPorts.Add(new PortData(typeof(UnityObject), isMulti: true));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var output = new AssetGroup();
            foreach (var i in portDataContainer.InputPorts)
                output.UnionWith(container.Get(i, AssetGroup.combineAssetGroup));
            container.Set(portDataContainer.OutputPorts[0], output);
        }

        sealed class CombineNodeEditor : INodeProcessorEditor
        {
            public bool UseDefaultVisualElement => false;

            public void OnEnable() { }
            public void OnDisable() { }

            public void OnGUI(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty)
            {
                EditorGUIUtility.wideMode = false;
                using (var m_TypeProperty = processorProperty.FindPropertyRelative(nameof(m_Type)))
                {
                    EditorGUI.BeginChangeCheck();
                    var newValue = UnityObjectTypeUtility.AssetTypePopup(m_TypeProperty.stringValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_TypeProperty.stringValue = newValue;
                        for (var i = 0; i < inputPortsProperty.arraySize; ++i)
                            using (var element = inputPortsProperty.GetArrayElementAtIndex(i))
                            using (var m_PortTypeProperty = element.FindPropertyRelative("m_PortType"))
                                m_PortTypeProperty.stringValue = newValue;
                        using (var element = outputPortsProperty.GetArrayElementAtIndex(0))
                        using (var m_PortTypeProperty = element.FindPropertyRelative("m_PortType"))
                            m_PortTypeProperty.stringValue = newValue;
                    }
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("+"))
                        ++inputPortsProperty.arraySize;
                    using (new EditorGUI.DisabledScope(inputPortsProperty.arraySize < 2))
                        if (GUILayout.Button("-"))
                            --inputPortsProperty.arraySize;
                }
            }
        }
    }
}
