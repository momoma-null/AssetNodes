using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using UnityObject = UnityEngine.Object;
using static UnityEngine.Object;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Modify/Component")]
    sealed class ModifyComponentNode : INodeProcessor, IAdditionalAssetHolder
    {
        sealed class ModifyComponentNodeEditor : INodeProcessorEditor
        {
            [NodeProcessorEditorFactory]
            static void Entry(IEntryDelegate<GenerateNodeProcessorEditor> factories)
            {
                factories.Add(typeof(ModifyComponentNode), (data, property, inputProperty, outputProperty) => new ModifyComponentNodeEditor(property));
            }

            readonly SerializedProperty _IncludeChildrenProperty;
            readonly SerializedProperty _PresetProperty;

            Preset m_Preset;
            Editor m_CachedEditor;
            string m_OldMenuPath = "";

            public bool UseDefaultVisualElement => false;

            public ModifyComponentNodeEditor(SerializedProperty processorProperty)
            {
                _IncludeChildrenProperty = processorProperty.FindPropertyRelative(nameof(m_IncludeChildren));
                _PresetProperty = processorProperty.FindPropertyRelative(nameof(m_Preset));
            }

            public void OnEnable()
            {
                if (m_CachedEditor == null)
                {
                    m_CachedEditor = Editor.CreateEditor(_PresetProperty.objectReferenceValue);
                }
            }

            public void OnDisable()
            {
                if (m_CachedEditor != null)
                {
                    DestroyImmediate(m_CachedEditor);
                    m_CachedEditor = null;
                }
            }

            public void OnGUI()
            {
                EditorGUILayout.PropertyField(_IncludeChildrenProperty);
                if (_PresetProperty.objectReferenceValue is Preset preset)
                {
                    using (var presetSo = new SerializedObject(preset))
                    using (var m_NativeTypeIDProperty = presetSo.FindProperty("m_TargetType.m_NativeTypeID"))
                    using (var m_ManagedTypePPtrProperty = presetSo.FindProperty("m_TargetType.m_ManagedTypePPtr"))
                    {
                        var menuPath = (m_ManagedTypePPtrProperty.objectReferenceValue is MonoScript monoScript) ? UnityObjectTypeUtility.GetMenuPath(monoScript) : UnityObjectTypeUtility.GetMenuPath(m_NativeTypeIDProperty.intValue);
                        EditorGUI.BeginChangeCheck();
                        menuPath = UnityObjectTypeUtility.ComponentTypePopup(menuPath, true);
                        if (menuPath != m_OldMenuPath)
                        {
                            if (m_CachedEditor != null)
                                DestroyImmediate(m_CachedEditor);
                            m_CachedEditor = null;
                        }
                        m_OldMenuPath = menuPath;
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (UnityObjectTypeUtility.TryGetComponentTypeFromMenuPath(menuPath, out var type))
                            {
                                var go = new GameObject();
                                try
                                {
                                    if (!go.TryGetComponent(type, out var comp))
                                        comp = go.AddComponent(type);
                                    var tempPreset = new Preset(comp) { hideFlags = HideFlags.HideInHierarchy };
                                    try
                                    {
                                        using (var tempSo = new SerializedObject(tempPreset))
                                        using (var iterator = tempSo.GetIterator())
                                        {
                                            iterator.Next(true);
                                            while (true)
                                            {
                                                presetSo.CopyFromSerializedProperty(iterator);
                                                if (!iterator.Next(false))
                                                    break;
                                            }
                                            presetSo.ApplyModifiedProperties();
                                        }
                                    }
                                    finally
                                    {
                                        DestroyImmediate(tempPreset);
                                    }
                                }
                                finally
                                {
                                    DestroyImmediate(go);
                                }
                            }
                        }
                    }
                    Editor.CreateCachedEditor(preset, null, ref m_CachedEditor);
                    m_CachedEditor.OnInspectorGUI();
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
        Preset m_Preset = null;

        public IEnumerable<UnityObject> Assets
        {
            get
            {
                if (m_Preset == null)
                {
                    var go = new GameObject();
                    try
                    {
                        m_Preset = new Preset(go.transform);
                        m_Preset.hideFlags = HideFlags.HideInHierarchy;
                    }
                    finally
                    {
                        DestroyImmediate(go);
                    }
                }
                return new UnityObject[] { m_Preset };
            }
        }

        public void OnClone()
        {
            foreach (Preset i in this.CloneAssets())
                m_Preset = i;
        }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<GameObject>(isMulti: true);
            portDataContainer.AddOutputPort<GameObject>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            if (m_Preset != null)
            {
                foreach (var assets in assetGroup)
                {
                    if ((assets.MainAsset.hideFlags & HideFlags.NotEditable) != 0 || !(assets.MainAsset is GameObject root))
                        continue;
                    foreach (var component in m_IncludeChildren ? assets.GetAssetsFromType<Component>() : root.GetComponents<Component>())
                        if (m_Preset.CanBeAppliedTo(component))
                            m_Preset.ApplyTo(component);
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
