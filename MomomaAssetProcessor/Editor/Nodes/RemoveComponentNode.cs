using System;
using UnityEditor;
using UnityEngine;

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
                _RegexProperty = processorProperty.FindPropertyRelative(nameof(m_RegexPattern));
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

        sealed class Modifier : IPrefabModifier
        {
            readonly RemoveComponentNode node;
            readonly Type componentType;

            public bool IncludeChildren => node.m_IncludeChildren;
            public string RegexPattern => node.m_RegexPattern;

            public Modifier(RemoveComponentNode node, Type componentType)
            {
                this.node = node;
                this.componentType = componentType;
            }

            public void Modify(GameObject go)
            {
                if (go.TryGetComponent(componentType, out var comp))
                    UnityEngine.Object.DestroyImmediate(comp, true);
            }
        }

        RemoveComponentNode() { }

        [SerializeField]
        bool m_IncludeChildren = false;
        [SerializeField]
        string m_RegexPattern = string.Empty;
        [SerializeField]
        string m_MenuPath = "";

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            if (UnityObjectTypeUtility.TryGetComponentTypeFromMenuPath(m_MenuPath, out var componentType))
                new Modifier(this, componentType).ModifyPrefab(assetGroup);
            container.SetOutput(0, assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
