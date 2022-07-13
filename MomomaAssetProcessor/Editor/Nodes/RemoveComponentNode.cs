using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Modify/Remove Component")]
    sealed class RemoveComponentNode : INodeProcessor
    {
        sealed class RemoveComponentNodeEditor : INodeProcessorEditor
        {
            [NodeProcessorEditorFactory]
            static void Entry()
            {
                NodeProcessorEditorFactory.EntryEditorFactory<RemoveComponentNode>((data, serializedPropertyList) => new RemoveComponentNodeEditor(serializedPropertyList.GetProcessorProperty()));
            }

            readonly SerializedProperty _IncludeChildrenProperty;
            readonly SerializedProperty _RegexProperty;
            readonly SerializedProperty _MenuPathProperty;

            public bool UseDefaultVisualElement => false;

            RemoveComponentNodeEditor(SerializedProperty processorProperty)
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

        RemoveComponentNode() { }

        [SerializeField]
        bool m_IncludeChildren = false;
        [SerializeField]
        string m_Regex = "";
        [SerializeField]
        string m_MenuPath = "";

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
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
                                if (go.TryGetComponent(componentType, out var comp))
                                    DestroyImmediate(comp, true);
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
