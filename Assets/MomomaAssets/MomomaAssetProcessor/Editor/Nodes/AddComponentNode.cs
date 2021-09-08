using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Modify/Add Component")]
    sealed class AddComponentNode : INodeProcessor
    {
        sealed class AddComponentNodeEditor : INodeProcessorEditor
        {
            public bool UseDefaultVisualElement => false;

            public void OnEnable() { }
            public void OnDisable() { }

            public void OnGUI(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty)
            {
                using (var m_IncludeChildrenProperty = processorProperty.FindPropertyRelative(nameof(m_IncludeChildren)))
                using (var m_RegexProperty = processorProperty.FindPropertyRelative(nameof(m_Regex)))
                using (var m_MenuPathProperty = processorProperty.FindPropertyRelative(nameof(m_MenuPath)))
                {
                    EditorGUILayout.PropertyField(m_IncludeChildrenProperty);
                    EditorGUILayout.PropertyField(m_RegexProperty);
                    EditorGUI.BeginChangeCheck();
                    var newPath = UnityObjectTypeUtility.ComponentTypePopup(m_MenuPathProperty.stringValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_MenuPathProperty.stringValue = newPath;
                    }
                }
            }
        }

        static AddComponentNode()
        {
            INodeDataUtility.AddConstructor(() => new AddComponentNode());
        }

        AddComponentNode() { }

        [SerializeField]
        bool m_IncludeChildren = false;
        [SerializeField]
        string m_Regex = "";
        [SerializeField]
        string m_MenuPath = "";

        public INodeProcessorEditor ProcessorEditor { get; } = new AddComponentNodeEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(GameObject), isMulti: true));
            portDataContainer.OutputPorts.Add(new PortData(typeof(GameObject), isMulti: true));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            if (UnityObjectTypeUtility.TryGetComponentTypeFromMenuPath(m_MenuPath, out var componentType))
            {
                var regex = new Regex(m_Regex);
                foreach (var assets in assetGroup)
                {
                    if ((assets.MainAsset.hideFlags & HideFlags.NotEditable) != 0 || !(assets.MainAsset is GameObject))
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
    }
}
