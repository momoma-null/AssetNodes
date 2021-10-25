using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;
using ReorderableList = UnityEditorInternal.ReorderableList;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Group/Group by Type")]
    sealed class GroupByTypeNode : INodeProcessor
    {
        sealed class GroupByTypeNodeEditor : INodeProcessorEditor
        {
            [NodeProcessorEditorFactory]
            static void Entry(IEntryDelegate<GenerateNodeProcessorEditor> factories)
            {
                factories.Add(typeof(GroupByTypeNode), (data, property, inputProperty, outputProperty) => new GroupByTypeNodeEditor(property, inputProperty, outputProperty));
            }

            readonly ReorderableList _ReorderableList;
            readonly SerializedProperty _RegexsProperty;
            readonly SerializedProperty _OutputPortsProperty;

            public bool UseDefaultVisualElement => false;

            GroupByTypeNodeEditor(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty)
            {
                _ReorderableList = new ReorderableList(new List<string>(), typeof(string), true, false, true, true);
                _ReorderableList.drawElementCallback = DrawElement;
                _ReorderableList.onReorderCallbackWithDetails = Reorder;
                _ReorderableList.onAddCallback = Add;
                _ReorderableList.onRemoveCallback = Remove;
                _ReorderableList.onCanRemoveCallback = CanRemove;
                _RegexsProperty = processorProperty.FindPropertyRelative(nameof(m_Regexes));
                _OutputPortsProperty = outputPortsProperty;
                for (var i = 0; i < _OutputPortsProperty.arraySize; ++i)
                        _ReorderableList.list.Add(null);
            }

            public void OnEnable() { }
            public void OnDisable() { }

            public void OnGUI()
            {
                if (_OutputPortsProperty.arraySize != _ReorderableList.count)
                {
                    _ReorderableList.list.Clear();
                    for (var i = 0; i < _OutputPortsProperty.arraySize; ++i)
                        _ReorderableList.list.Add(null);
                }
                _ReorderableList.DoLayoutList();
            }

            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                rect.width *= 0.5f;
                using (var regexProperty = _RegexsProperty.GetArrayElementAtIndex(index))
                    regexProperty.stringValue = EditorGUI.TextField(rect, regexProperty.stringValue);
                rect.x += rect.width;
                using (var element = _OutputPortsProperty.GetArrayElementAtIndex(index))
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
                _RegexsProperty.MoveArrayElement(oldIndex, newIndex);
                _OutputPortsProperty.MoveArrayElement(oldIndex, newIndex);
            }

            void Add(ReorderableList list)
            {
                ++_OutputPortsProperty.arraySize;
                _RegexsProperty.arraySize = _OutputPortsProperty.arraySize;
                list.list.Add(null);
            }

            void Remove(ReorderableList list)
            {
                if (_OutputPortsProperty.arraySize == 0)
                    return;
                _RegexsProperty.DeleteArrayElementAtIndex(list.index);
                _OutputPortsProperty.DeleteArrayElementAtIndex(list.index);
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
