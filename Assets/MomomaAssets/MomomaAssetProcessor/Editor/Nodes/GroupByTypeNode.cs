using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;
using ReorderableList = UnityEditorInternal.ReorderableList;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Group/Group by Type")]
    sealed class GroupByTypeNode : INodeProcessor
    {
        sealed class GroupByTypeNodeEditor : INodeProcessorEditor
        {
            ReorderableList? m_ReorderableList;

            SerializedProperty? m_RegexsProperty;
            SerializedProperty? m_OutputPortsProperty;

            public bool UseDefaultVisualElement => false;

            public void OnEnable() { }
            public void OnDisable() { }

            public void OnGUI(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty)
            {
                if (m_ReorderableList == null)
                {
                    m_ReorderableList = new ReorderableList(new List<string>(), typeof(string), true, false, true, true);
                    m_ReorderableList.drawElementCallback = DrawElement;
                    m_ReorderableList.onReorderCallbackWithDetails = Reorder;
                    m_ReorderableList.onAddCallback = Add;
                    m_ReorderableList.onRemoveCallback = Remove;
                    m_ReorderableList.onCanRemoveCallback = CanRemove;
                }
                m_RegexsProperty = processorProperty.FindPropertyRelative(nameof(m_Regexes));
                m_OutputPortsProperty = outputPortsProperty;
                if (m_OutputPortsProperty.arraySize != m_ReorderableList.count)
                {
                    m_ReorderableList.list.Clear();
                    for (var i = 0; i < m_OutputPortsProperty.arraySize; ++i)
                        m_ReorderableList.list.Add(null);
                }
                m_ReorderableList.DoLayoutList();
            }

            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (m_RegexsProperty == null || m_OutputPortsProperty == null)
                    return;
                rect.width *= 0.5f;
                using (var regexProperty = m_RegexsProperty.GetArrayElementAtIndex(index))
                    regexProperty.stringValue = EditorGUI.TextField(rect, regexProperty.stringValue);
                rect.x += rect.width;
                using (var element = m_OutputPortsProperty.GetArrayElementAtIndex(index))
                using (var portTypeProperty = element.FindPropertyRelative("m_PortType"))
                {
                    EditorGUI.BeginChangeCheck();
                    var newValue = UnityObjectTypeUtility.AssetTypePopup(rect, portTypeProperty.stringValue);
                    if (EditorGUI.EndChangeCheck())
                        portTypeProperty.stringValue = newValue;
                }
            }

            void Reorder(ReorderableList list, int oldIndex, int newIndex)
            {
                m_RegexsProperty?.MoveArrayElement(oldIndex, newIndex);
                m_OutputPortsProperty?.MoveArrayElement(oldIndex, newIndex);
            }

            void Add(ReorderableList list)
            {
                if (m_RegexsProperty == null || m_OutputPortsProperty == null)
                    return;
                ++m_OutputPortsProperty.arraySize;
                m_RegexsProperty.arraySize = m_OutputPortsProperty.arraySize;
                list.list.Add(null);
            }

            void Remove(ReorderableList list)
            {
                if (m_RegexsProperty == null || m_OutputPortsProperty == null)
                    return;
                if (m_OutputPortsProperty.arraySize == 0)
                    return;
                m_RegexsProperty.DeleteArrayElementAtIndex(list.index);
                m_OutputPortsProperty.DeleteArrayElementAtIndex(list.index);
                list.list.RemoveAt(list.index);
            }

            bool CanRemove(ReorderableList list)
            {
                return list.count > 1;
            }
        }

        static GroupByTypeNode()
        {
            INodeDataUtility.AddConstructor(() => new GroupByTypeNode());
        }

        GroupByTypeNode() { }

        [SerializeField]
        string[] m_Regexes = new string[1];

        public INodeProcessorEditor ProcessorEditor { get; } = new GroupByTypeNodeEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<UnityObject>(isMulti: true);
            portDataContainer.AddOutputPort<UnityObject>(isMulti: true);
        }

        sealed class Output
        {
            public AssetGroup assetGroup { get; } = new AssetGroup();
            public Regex regex { get; }
            public AssetTypeData assetTypeData { get; }

            public Output(string regex, string typeName)
            {
                this.regex = new Regex(regex);
                assetTypeData = UnityObjectTypeUtility.GetAssetTypeData(typeName);
            }
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            var outputs = new Output[portDataContainer.OutputPorts.Count];
            for (var i = 0; i < outputs.Length; ++i)
                outputs[i] = new Output(m_Regexes[i] ?? string.Empty, portDataContainer.OutputPorts[i].PortTypeName);
            foreach (var assets in assetGroup)
            {
                foreach (var o in outputs)
                {
                    if (o.assetTypeData.AssetType == assets.MainAssetType || (assets.Importer != null && o.assetTypeData.IsTarget(assets.Importer)))
                    {
                        if (o.regex.Match(assets.AssetPath).Success)
                        {
                            o.assetGroup.Add(assets);
                            break;
                        }
                    }
                }
            }
            for (var i = 0; i < outputs.Length; ++i)
                container.Set(portDataContainer.OutputPorts[i], outputs[i].assetGroup);
        }
    }
}
