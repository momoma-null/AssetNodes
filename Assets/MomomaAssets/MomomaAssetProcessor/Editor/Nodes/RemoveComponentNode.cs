using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Object;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Modify/Remove Component")]
    sealed class RemoveComponentNode : INodeProcessor
    {
        sealed class RemoveComponentNodeEditor : INodeProcessorEditor
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

        static RemoveComponentNode()
        {
            INodeDataUtility.AddConstructor(() => new RemoveComponentNode());
        }

        RemoveComponentNode() { }

        [SerializeField]
        bool m_IncludeChildren = false;
        [SerializeField]
        string m_Regex = "";
        [SerializeField]
        string m_MenuPath = "";

        public INodeProcessorEditor ProcessorEditor { get; } = new RemoveComponentNodeEditor();

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
                    var root = PrefabUtility.LoadPrefabContents(assets.AssetPath);
                    try
                    {
                        foreach (var go in m_IncludeChildren ? root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject) : new[] { root })
                        {
                            if (regex.Match(go.name).Success)
                            {
                                if (go.TryGetComponent(componentType, out var comp))
                                    DestroyImmediate(comp, true);
                            }
                        }
                    }
                    finally
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, assets.AssetPath);
                        PrefabUtility.UnloadPrefabContents(root);
                    }
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
 