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
    [CreateElement(typeof(AssetProcessorGUI), "Group/Group by Type")]
    sealed class GroupByTypeNode : INodeProcessor
    {
        sealed class GroupByTypeNodeEditor : INodeProcessorEditor
        {
            [NodeProcessorEditorFactory]
            static void Entry()
            {
                NodeProcessorEditorFactory.EntryEditorFactory<GroupByTypeNode>((data, serializedNodeProcessor) => new GroupByTypeNodeEditor(serializedNodeProcessor));
            }

            readonly ReorderableList _ReorderableList;
            readonly SerializedProperty _RegexsProperty;
            readonly SerializedPropertyList _OutputPorts;

            public bool UseDefaultVisualElement => false;

            GroupByTypeNodeEditor(SerializedNodeProcessor serializedNodeProcessor)
            {
                _ReorderableList = new ReorderableList(new List<string>(), typeof(string), true, false, true, true);
                _ReorderableList.drawElementCallback = DrawElement;
                _ReorderableList.onReorderCallbackWithDetails = Reorder;
                _ReorderableList.onAddCallback = Add;
                _ReorderableList.onRemoveCallback = Remove;
                _ReorderableList.onCanRemoveCallback = CanRemove;
                _RegexsProperty = serializedNodeProcessor.GetProcessorProperty().FindPropertyRelative(nameof(m_Regexes));
                _OutputPorts = serializedNodeProcessor.OutputPorts;
                for (var i = 0; i < _OutputPorts.Count; ++i)
                    _ReorderableList.list.Add(null);
            }

            public void Dispose() { }

            public void OnGUI()
            {
                if (_OutputPorts.Count != _ReorderableList.count)
                {
                    _ReorderableList.list.Clear();
                    for (var i = 0; i < _OutputPorts.Count; ++i)
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
                EditorGUI.BeginChangeCheck();
                var portType = PortData.GetPortType(_OutputPorts, index);
                var newValue = UnityObjectTypeUtility.AssetTypePopup(rect, portType);
                if (EditorGUI.EndChangeCheck())
                    PortData.SetPortType(_OutputPorts, index, newValue);
            }

            void Reorder(ReorderableList list, int oldIndex, int newIndex)
            {
                _RegexsProperty.MoveArrayElement(oldIndex, newIndex);
                _OutputPorts.Move(oldIndex, newIndex);
            }

            void Add(ReorderableList list)
            {
                _OutputPorts.Add();
                _RegexsProperty.arraySize = _OutputPorts.Count;
                list.list.Add(null);
            }

            void Remove(ReorderableList list)
            {
                if (_OutputPorts.Count == 0)
                    return;
                _RegexsProperty.DeleteArrayElementAtIndex(list.index);
                _OutputPorts.RemoveAt(list.index);
                list.list.RemoveAt(list.index);
            }

            bool CanRemove(ReorderableList list)
            {
                return list.count > 1;
            }
        }

        GroupByTypeNode() { }

        [SerializeField]
        string[] m_Regexes = new string[1];

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
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
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroupPortDefinition.Default);
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

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
