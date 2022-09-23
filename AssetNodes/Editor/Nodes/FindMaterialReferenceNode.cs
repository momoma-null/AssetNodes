using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

//#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Validate/Material Reference")]
    sealed class FindMaterialReferenceNode : INodeProcessor
    {
        enum ComparisonMode
        {
            Null,
            Any
        }

        [Serializable]
        struct TextureReference
        {
            public string _PropertyName;
            public ComparisonMode _ComparisonMode;
        }

        sealed class FindMaterialReferenceNodeEditor : INodeProcessorEditor
        {
            [CustomPropertyDrawer(typeof(TextureReference))]
            sealed class TextureReferenceDrawer : PropertyDrawer
            {
                static GUIContent[] s_PropertyNames = Array.Empty<GUIContent>();
                static int[] s_PropertyHashes = Array.Empty<int>();

                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    using (var propertyNameProperty = property.FindPropertyRelative(nameof(TextureReference._PropertyName)))
                    using (var comparisonModeProperty = property.FindPropertyRelative(nameof(TextureReference._ComparisonMode)))
                    {
                        position.width *= 0.5f;
                        EditorGUI.BeginChangeCheck();
                        var propertyHash = EditorGUI.IntPopup(position, propertyNameProperty.stringValue.GetHashCode(), s_PropertyNames, s_PropertyHashes);
                        if (EditorGUI.EndChangeCheck())
                        {
                            var index = Array.IndexOf(s_PropertyHashes, propertyHash);
                            if (0 <= index && index < s_PropertyNames.Length)
                            {
                                propertyNameProperty.stringValue = s_PropertyNames[index].text;
                            }
                        }
                        position.x += position.width;
                        EditorGUI.PropertyField(position, comparisonModeProperty, GUIContent.none);
                    }
                }

                public sealed class PopupScope : IDisposable
                {
                    readonly GUIContent[] _PropertyNames;
                    readonly int[] _PropertyHashes;

                    public PopupScope(GUIContent[] propertyNames, int[] propertyHashes)
                    {
                        _PropertyNames = s_PropertyNames;
                        _PropertyHashes = s_PropertyHashes;
                        s_PropertyNames = propertyNames;
                        s_PropertyHashes = propertyHashes;
                    }

                    void IDisposable.Dispose()
                    {
                        s_PropertyNames = _PropertyNames;
                        s_PropertyHashes = _PropertyHashes;
                    }
                }
            }

            [NodeProcessorEditorFactory]
            static void Entry()
            {
                NodeProcessorEditorFactory.EntryEditorFactory<FindMaterialReferenceNode>((data, serializedNodeProcessor) => new FindMaterialReferenceNodeEditor(serializedNodeProcessor.GetProcessorProperty()));
            }

            readonly SerializedProperty _ShaderProperty;
            readonly SerializedProperty _TextureReferencesProperty;

            GUIContent[] _PropertyNames = Array.Empty<GUIContent>();
            int[] _PropertyHashes = Array.Empty<int>();
            int _PropertiesHash;

            public bool UseDefaultVisualElement => false;

            FindMaterialReferenceNodeEditor(SerializedProperty processorProperty)
            {
                _ShaderProperty = processorProperty.FindPropertyRelative(nameof(m_Shader));
                _TextureReferencesProperty = processorProperty.FindPropertyRelative(nameof(m_TextureReferences));
            }

            public void Dispose() { }

            public void OnGUI()
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_ShaderProperty);
                var shader = _ShaderProperty.objectReferenceValue as Shader;
                var currentHash = GetPropertiesHash(shader);
                if (EditorGUI.EndChangeCheck() || _PropertiesHash != currentHash)
                {
                    _PropertiesHash = currentHash;
                    RebuildPopup(shader, out _PropertyNames, out _PropertyHashes);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    _TextureReferencesProperty.ClearArray();
                }
                using (new TextureReferenceDrawer.PopupScope(_PropertyNames, _PropertyHashes))
                {
                    EditorGUILayout.PropertyField(_TextureReferencesProperty);
                }
            }

            static int GetPropertiesHash(Shader shader)
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

            static void RebuildPopup(Shader shader, out GUIContent[] contents, out int[] hashes)
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

        FindMaterialReferenceNode() { }

        [SerializeField]
        Shader m_Shader;
        [SerializeField]
        TextureReference[] m_TextureReferences = Array.Empty<TextureReference>();

        public Color HeaderColor => ColorDefinition.ValidateNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
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
                            var found = false;
                            switch (reference._ComparisonMode)
                            {
                                case ComparisonMode.Null: found = tex == null; break;
                                case ComparisonMode.Any: found = tex != null; break;
                                default: throw new ArgumentOutOfRangeException(nameof(reference._ComparisonMode));
                            }
                            if (found)
                            {
                                Debug.LogWarning($"Material reference : {material.name}, {assetData.AssetPath}, {reference._PropertyName}", material);
                                outAssetGroup.Add(assetData);
                            }
                        }
                    }
                }
            }
            container.SetOutput(0, outAssetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
