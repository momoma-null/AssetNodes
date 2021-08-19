using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.Compilation;
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
                    if (m_PresetProperty.objectReferenceValue is Preset preset)
                    {
                        using (var presetSo = new SerializedObject(preset))
                        using (var m_NativeTypeIDProperty = presetSo.FindProperty("m_TargetType.m_NativeTypeID"))
                        using (var m_ManagedTypePPtrProperty = presetSo.FindProperty("m_TargetType.m_ManagedTypePPtr"))
                        {
                            var menuPath = (m_ManagedTypePPtrProperty.objectReferenceValue is MonoScript monoScript) ? UnityObjectTypeUtility.GetMenuPath(monoScript) : UnityObjectTypeUtility.GetMenuPath(m_NativeTypeIDProperty.intValue);
                            EditorGUI.BeginChangeCheck();
                            menuPath = UnityObjectTypeUtility.ComponentTypePopup(menuPath);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (UnityObjectTypeUtility.TryGetComponentTypeFromMenuPath(menuPath, out var type))
                                {
                                    var go = new GameObject();
                                    try
                                    {
                                        var comp = go.AddComponent(type);
                                        var tempPreset = new Preset(comp);
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
                                                DestroyImmediate(m_CachedEditor);
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
                    EditorGUILayout.PropertyField(m_IncludeChildrenProperty);
                }
            }
        }

        sealed class MonoScriptsInfo
        {
            readonly HashSet<MonoScript> monoScripts;
            readonly HashSet<Type> monoBehaviours;
            public string[] MonoBehaviourDisplayNames { get; }

            public MonoScriptsInfo()
            {
                monoScripts = new HashSet<MonoScript>(CompilationPipeline.GetAssemblies().SelectMany(asm => asm.sourceFiles).Select(path => AssetDatabase.LoadAssetAtPath<MonoScript>(path)));
                monoBehaviours = new HashSet<Type>();
                var monoBehaviourType = typeof(MonoBehaviour);
                foreach (var i in monoScripts)
                {
                    var type = i.GetClass();
                    if (type != null && type.IsSubclassOf(monoBehaviourType))
                        monoBehaviours.Add(type);
                }
                MonoBehaviourDisplayNames = monoBehaviours.Select(t => t.FullName.Replace('.', '/')).ToArray();
            }
        }

        static ModifyComponentNode()
        {
            INodeDataUtility.AddConstructor(() => new ModifyComponentNode());
        }

        ModifyComponentNode() { }

        static MonoScriptsInfo? s_MonoScriptsInfo;
        static MonoScriptsInfo SafeMonoScriptsInfo => s_MonoScriptsInfo ?? (s_MonoScriptsInfo = new MonoScriptsInfo());

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
