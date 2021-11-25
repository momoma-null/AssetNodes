using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Modify/Add Component")]
    sealed class AddComponentNode : INodeProcessor
    {
        sealed class AddComponentNodeEditor : INodeProcessorEditor
        {
            [NodeProcessorEditorFactory]
            static void Entry()
            {
                NodeProcessorEditorFactory.EntryEditorFactory<AddComponentNode>((data, serializedNodeProcessor) => new AddComponentNodeEditor(serializedNodeProcessor.GetProcessorProperty()));
            }

            readonly SerializedProperty _IncludeChildrenProperty;
            readonly SerializedProperty _RegexProperty;
            readonly SerializedProperty _MenuPathProperty;

            public bool UseDefaultVisualElement => false;

            AddComponentNodeEditor(SerializedProperty processorProperty)
            {
                _IncludeChildrenProperty = processorProperty.FindPropertyRelative(nameof(m_IncludeChildren));
                _RegexProperty = processorProperty.FindPropertyRelative(nameof(m_Regex));
                _MenuPathProperty = processorProperty.FindPropertyRelative(nameof(m_MenuPath));
            }

            public void Dispose() { }

            public void OnGUI()
            {
                EditorGUILayout.PropertyField(_IncludeChildrenProperty);
                EditorGUILayout.PropertyField(_RegexProperty);
                EditorGUI.BeginChangeCheck();
                var newPath = UnityObjectTypeUtility.ComponentTypePopup(_MenuPathProperty.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    _MenuPathProperty.stringValue = newPath;
                }
            }
        }

        AddComponentNode() { }

        [SerializeField]
        bool m_IncludeChildren = false;
        [SerializeField]
        string m_Regex = "";
        [SerializeField]
        string m_MenuPath = "";

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<GameObject>(isMulti: true);
            portDataContainer.AddOutputPort<GameObject>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            if (UnityObjectTypeUtility.TryGetComponentTypeFromMenuPath(m_MenuPath, out var componentType))
            {
                var regex = new Regex(m_Regex);
                foreach (var assets in assetGroup)
                {
                    if (!(assets.MainAssetType == typeof(GameObject)) || (assets.MainAsset.hideFlags & HideFlags.NotEditable) != 0)
                        continue;
                    using (var scope = new PrefabUtility.EditPrefabContentsScope(assets.AssetPath))
                    {
                        var root = scope.prefabContentsRoot;
                        foreach (var go in m_IncludeChildren ? root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject) : new[] { root })
                        {
                            if (regex.Match(go.name).Success)
                            {
                                if (go.GetComponent(componentType) == null)
                                    go.AddComponent(componentType);
                            }
                        }
                    }
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
