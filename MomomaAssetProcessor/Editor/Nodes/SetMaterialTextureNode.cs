using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Modify/Material Texture")]
    sealed class SetMaterialTextureNode : INodeProcessor
    {
        sealed class SetMaterialTextureNodeEditor : INodeProcessorEditor
        {
            [NodeProcessorEditorFactory]
            static void Entry()
            {
                NodeProcessorEditorFactory.EntryEditorFactory<SetMaterialTextureNode>((data, serializedPropertyList) => new SetMaterialTextureNodeEditor(serializedPropertyList.GetProcessorProperty()));
            }

            public bool UseDefaultVisualElement => false;

            readonly SerializedProperty _ShaderProperty;
            readonly SerializedProperty _TexturePropertiesProperty;

            GUIContent[] displayNames = Array.Empty<GUIContent>();
            string[] propertyNames = Array.Empty<string>();

            public SetMaterialTextureNodeEditor(SerializedProperty processorProperty)
            {
                _ShaderProperty = processorProperty.FindPropertyRelative(nameof(m_Shader));
                _TexturePropertiesProperty = processorProperty.FindPropertyRelative(nameof(m_TextureProperties));
                if (_ShaderProperty.objectReferenceValue is Shader shader)
                {
                    UpdateTextureProperties(shader);
                }
            }

            public void Dispose() { }

            public void OnGUI()
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_ShaderProperty);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_ShaderProperty.objectReferenceValue is Shader shader)
                    {
                        UpdateTextureProperties(shader);
                    }
                }
                if (_ShaderProperty.objectReferenceValue == null)
                    return;
                using (new PropertyNameProperty.OptionScope(displayNames, propertyNames))
                {
                    EditorGUILayout.PropertyField(_TexturePropertiesProperty);
                }
            }

            void UpdateTextureProperties(Shader shader)
            {
                var count = shader.GetPropertyCount();
                var displayNameList = new List<GUIContent>(count);
                var propertyNameList = new List<string>(count);
                for (var i = 0; i < count; ++i)
                {
                    if (shader.GetPropertyType(i) == ShaderPropertyType.Texture
                        && (shader.GetPropertyFlags(i) & (ShaderPropertyFlags.PerRendererData
                                                          | ShaderPropertyFlags.NonModifiableTextureData)) == 0)
                    {
                        displayNameList.Add(EditorGUIUtility.TrTextContent(shader.GetPropertyDescription(i)));
                        propertyNameList.Add(shader.GetPropertyName(i));
                    }
                }
                displayNames = displayNameList.ToArray();
                propertyNames = propertyNameList.ToArray();
            }
        }

        sealed class PropertyNameAttribute : PropertyAttribute
        {
            public PropertyNameAttribute() { }
        }

        [CustomPropertyDrawer(typeof(PropertyNameAttribute))]
        sealed class PropertyNameProperty : PropertyDrawer
        {
            static OptionInfo s_Option = OptionInfo.Empty;

            sealed class OptionInfo
            {
                public static OptionInfo Empty = new OptionInfo();

                public GUIContent[] Options = Array.Empty<GUIContent>();
                public string[] PropertyNames = Array.Empty<string>();
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.Popup(position, label, Array.FindIndex(s_Option.PropertyNames, i => i == property.stringValue), s_Option.Options);
                if (EditorGUI.EndChangeCheck() && -1 < newValue && newValue < s_Option.PropertyNames.Length)
                {
                    property.stringValue = s_Option.PropertyNames[newValue];
                }
            }

            public sealed class OptionScope : IDisposable
            {
                public OptionScope(GUIContent[] options, string[] propertyNames)
                {
                    s_Option.Options = options;
                    s_Option.PropertyNames = propertyNames;
                }

                void IDisposable.Dispose()
                {
                    s_Option.Options = Array.Empty<GUIContent>();
                    s_Option.PropertyNames = Array.Empty<string>();
                }
            }
        }

        [Serializable]
        sealed class TextureProperty
        {
            [PropertyName]
            [SerializeField]
            string m_PropertyName = string.Empty;
            [SerializeField]
            string m_RegexPattern = string.Empty;
            [SerializeField]
            string m_Replacement = string.Empty;

            public string PropertyName => m_PropertyName;
            public string RegexPattern => m_RegexPattern;
            public string Replacement => m_Replacement;
        }

        SetMaterialTextureNode() { }

        [SerializeField]
        Shader? m_Shader;
        [SerializeField]
        TextureProperty[] m_TextureProperties = Array.Empty<TextureProperty>();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default, "Materials");
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default, "Textures");
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            var textureGroup = container.GetInput(1, AssetGroupPortDefinition.Default);
            if (m_Shader != null)
            {
                var textures = textureGroup.SelectMany(i => i.GetAssetsFromType<Texture>()).ToDictionary(i => i.name);
                foreach (var assets in assetGroup)
                {
                    foreach (var material in assets.GetAssetsFromType<Material>())
                    {
                        if (material.shader != m_Shader)
                            continue;
                        foreach (var i in m_TextureProperties)
                        {
                            if (!material.HasProperty(i.PropertyName))
                                continue;
                            var texName = Regex.Replace(material.name, i.RegexPattern, i.Replacement);
                            if (textures.TryGetValue(texName, out var tex))
                                material.SetTexture(i.PropertyName, tex);
                        }
                        if (EditorUtility.IsDirty(material))
                            EditorUtility.SetDirty(material);
                    }
                }
            }
            container.SetOutput(0, assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
