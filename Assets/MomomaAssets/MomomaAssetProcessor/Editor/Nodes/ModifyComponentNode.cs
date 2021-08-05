using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Modify/Component")]
    sealed class ModifyComponentNode : INodeProcessor
    {
        sealed class ModifyComponentNodeEditor : INodeProcessorEditor
        {
            Editor? m_CachedEditor;

            public bool UseDefaultVisualElement => true;

            public void OnDestroy()
            {
                if (m_CachedEditor != null)
                    DestroyImmediate(m_CachedEditor);
            }

            public void OnGUI(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty)
            {
                using (var m_IncludeChildrenProperty = processorProperty.FindPropertyRelative(nameof(m_IncludeChildren)))
                using (var m_PresetProperty = processorProperty.FindPropertyRelative(nameof(m_Preset)))
                {
                    EditorGUILayout.PropertyField(m_IncludeChildrenProperty);
                    EditorGUILayout.PropertyField(m_PresetProperty);
                    var preset = m_PresetProperty.objectReferenceValue;
                    if (preset != null)
                    {
                        Editor.CreateCachedEditor(preset, null, ref m_CachedEditor);
                        m_CachedEditor.OnInspectorGUI();
                    }
                }
            }
        }

        static ModifyComponentNode()
        {
            INodeDataUtility.AddConstructor(() => new ModifyComponentNode());
        }

        ModifyComponentNode() { }

        [SerializeField]
        bool m_IncludeChildren = false;
        [SerializeField]
        Preset? m_Preset = null;

        public INodeProcessorEditor ProcessorEditor { get; } = new ModifyComponentNodeEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(GameObject)));
            portDataContainer.OutputPorts.Add(new PortData(typeof(GameObject)));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], this.NewAssetGroup);
            if (m_Preset != null)
            {
                foreach (var assets in assetGroup)
                {
                    if (m_IncludeChildren)
                    {
                        foreach (var component in assets.GetAssetsFromType<Component>())
                            if (m_Preset.CanBeAppliedTo(component))
                                m_Preset.ApplyTo(component);
                    }
                    else if (assets.MainAsset is GameObject root)
                    {
                        foreach (var component in root.GetComponents<Component>())
                            if (m_Preset.CanBeAppliedTo(component))
                                m_Preset.ApplyTo(component);
                    }
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
