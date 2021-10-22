using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Find/Material Reference")]
    sealed class FindMaterialReferenceNode : INodeProcessor
    {
        enum ComparisonMode
        {
            Null,
            Any
        }

        struct TextureReference
        {
            public string _PropertyName;
            public ComparisonMode _ComparisonMode;
        }

        sealed class FindMaterialReferenceNodeEditor : INodeProcessorEditor
        {
            [NodeProcessorEditorFactory]
            static void Entry(IEntryDelegate<GenerateNodeProcessorEditor> factories)
            {
                factories.Add(typeof(FindMaterialReferenceNode), (data, property, inputProperty, outputProperty) => new FindMaterialReferenceNodeEditor(property));
            }

            readonly SerializedProperty _ShaderProperty;
            readonly SerializedProperty _TextureReferencesProperty;

            GUIContent[] _ProeprtyNames = Array.Empty<GUIContent>();
            int[] _PropertyHashes = Array.Empty<int>();
            int _PropertiesHash;

            public bool UseDefaultVisualElement => false;

            FindMaterialReferenceNodeEditor(SerializedProperty processorProperty)
            {
                _ShaderProperty = processorProperty.FindPropertyRelative(nameof(m_Shader));
                _TextureReferencesProperty = processorProperty.FindPropertyRelative(nameof(m_TextureReferences));
            }

            public void OnEnable() { }
            public void OnDisable() { }
            public void OnGUI()
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_ShaderProperty);
                var shader = _ShaderProperty.objectReferenceValue as Shader;
                var currentHash = GetPropertiesHash(shader);
                if (EditorGUI.EndChangeCheck() || _PropertiesHash != currentHash)
                {
                    _PropertiesHash = currentHash;
                    RebuildPopup(shader, out _ProeprtyNames, out _PropertyHashes);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    _TextureReferencesProperty.ClearArray();
                }
                for (var i = 0; i < _TextureReferencesProperty.arraySize; ++i)
                {
                    using (var element = _TextureReferencesProperty.GetArrayElementAtIndex(i))
                    using (var propertyNameProperty = element.FindPropertyRelative(nameof(TextureReference._PropertyName)))
                    using (var comparisonModeProperty = element.FindPropertyRelative(nameof(TextureReference._ComparisonMode)))
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        var propertyHash = EditorGUILayout.IntPopup(propertyNameProperty.stringValue.GetHashCode(), _ProeprtyNames, _PropertyHashes);
                        if (EditorGUI.EndChangeCheck())
                        {
                            var index = Array.IndexOf(_PropertyHashes, propertyHash);
                            if (0 <= index && index < _ProeprtyNames.Length)
                            {
                                propertyNameProperty.stringValue = _ProeprtyNames[index].text;
                            }
                        }
                    }
                }
            }

            static int GetPropertiesHash(Shader? shader)
            {
                var hash = 0;
                if (shader != null)
                {
                    hash ^= shader.name.GetHashCode();
                    var propertyCount = shader.GetPropertyCount();
                    for (var i = 0; i < propertyCount; ++i)
                    {
                        var propertyType = shader.GetPropertyType(i);
                        if (propertyType == ShaderPropertyType.Texture)
                            hash ^= shader.GetPropertyName(i).GetHashCode();
                    }
                }
                return hash;
            }

            static void RebuildPopup(Shader? shader, out GUIContent[] contents, out int[] hashes)
            {
                var propertyNames = new HashSet<string>();
                if (shader != null)
                {
                    var propertyCount = shader.GetPropertyCount();
                    for (var i = 0; i < propertyCount; ++i)
                    {
                        var propertyType = shader.GetPropertyType(i);
                        if (propertyType == ShaderPropertyType.Texture)
                            propertyNames.Add(shader.GetPropertyName(i));
                    }
                }
                contents = propertyNames.Select(s => EditorGUIUtility.TrTextContent(s)).ToArray();
                hashes = Array.ConvertAll(contents, c => c.text.GetHashCode());
            }
        }

        static FindMaterialReferenceNode()
        {
            INodeDataUtility.AddConstructor(() => new FindMaterialReferenceNode());
        }

        FindMaterialReferenceNode() { }

        [SerializeField]
        Shader? m_Shader;
        [SerializeField]
        TextureReference[] m_TextureReferences = Array.Empty<TextureReference>();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<Material>(isMulti: true);
            portDataContainer.AddOutputPort<Material>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            var outAssetGroup = new AssetGroup();
            if (m_Shader != null)
            {
                foreach (var assetData in assetGroup)
                {
                    foreach (var material in assetData.GetAssetsFromType<Material>())
                    {
                        if (material.shader != m_Shader)
                            continue;
                        foreach (var reference in m_TextureReferences)
                        {
                            var tex = material.GetTexture(reference._PropertyName);
                            var found = reference._ComparisonMode switch
                            {
                                ComparisonMode.Null => tex == null,
                                ComparisonMode.Any => tex != null,
                                _ => throw new ArgumentOutOfRangeException(nameof(reference._ComparisonMode))
                            };
                            if (found)
                            {
                                Debug.LogWarning($"Material reference : {material.name}, {assetData.AssetPath}, {reference._PropertyName}", material);
                                outAssetGroup.Add(assetData);
                            }
                        }
                    }
                }
            }
            container.Set(portDataContainer.OutputPorts[0], outAssetGroup);
        }
    }
}
