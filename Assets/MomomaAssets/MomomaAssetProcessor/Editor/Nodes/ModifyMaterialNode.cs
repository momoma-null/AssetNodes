using System;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Object;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Modify/Material")]
    sealed class ModifyMaterialNode : INodeProcessor
    {
        interface IPropertyValue
        {
            bool Enabled { get; set; }
            void SetPropertyValue(Material mat);
            void CopyFromMaterialProperty(MaterialProperty property);
        }

        [Serializable]
        struct FloatValue : IPropertyValue
        {
            [SerializeField]
            bool enabled;
            [SerializeField]
            string propertyName;
            [SerializeField]
            float floatValue;

            public bool Enabled { get => enabled; set => enabled = value; }

            public void SetPropertyValue(Material mat)
            {
                if (mat.HasProperty(propertyName))
                    mat.SetFloat(propertyName, floatValue);
            }

            public void CopyFromMaterialProperty(MaterialProperty property)
            {
                propertyName = property.name;
                floatValue = property.floatValue;
            }
        }

        [Serializable]
        struct ColorValue : IPropertyValue
        {
            [SerializeField]
            bool enabled;
            [SerializeField]
            string propertyName;
            [SerializeField]
            Color colorValue;

            public bool Enabled { get => enabled; set => enabled = value; }

            public void SetPropertyValue(Material mat)
            {
                if (mat.HasProperty(propertyName))
                    mat.SetColor(propertyName, colorValue);
            }

            public void CopyFromMaterialProperty(MaterialProperty property)
            {
                propertyName = property.name;
                colorValue = property.colorValue;
            }
        }

        [Serializable]
        struct VectorValue : IPropertyValue
        {
            [SerializeField]
            bool enabled;
            [SerializeField]
            string propertyName;
            [SerializeField]
            Vector4 vectorValue;

            public bool Enabled { get => enabled; set => enabled = value; }

            public void SetPropertyValue(Material mat)
            {
                if (mat.HasProperty(propertyName))
                    mat.SetVector(propertyName, vectorValue);
            }

            public void CopyFromMaterialProperty(MaterialProperty property)
            {
                propertyName = property.name;
                vectorValue = property.vectorValue;
            }
        }

        [Serializable]
        struct TextureValue : IPropertyValue
        {
            [SerializeField]
            bool enabled;
            [SerializeField]
            string propertyName;
            [SerializeField]
            Texture objectReferenceValue;
            [SerializeField]
            Vector2 scale;
            [SerializeField]
            Vector2 offset;

            public bool Enabled { get => enabled; set => enabled = value; }

            public void SetPropertyValue(Material mat)
            {
                if (mat.HasProperty(propertyName))
                {
                    mat.SetTexture(propertyName, objectReferenceValue);
                    mat.SetTextureScale(propertyName, scale);
                    mat.SetTextureOffset(propertyName, offset);
                }
            }

            public void CopyFromMaterialProperty(MaterialProperty property)
            {
                propertyName = property.name;
                objectReferenceValue = property.textureValue;
                scale = new Vector2(property.textureScaleAndOffset.x, property.textureScaleAndOffset.y);
                offset = new Vector2(property.textureScaleAndOffset.z, property.textureScaleAndOffset.w);
            }
        }

        sealed class ModifyMaterialNodeEditor : INodeProcessorEditor
        {
            [NodeProcessorEditorFactory]
            static void Entry(IEntryDelegate<GenerateNodeProcessorEditor> factories)
            {
                factories.Add(typeof(ModifyMaterialNode), (data, property, inputProperty, outputProperty) => data is ModifyMaterialNode node ? new ModifyMaterialNodeEditor(node, property) : throw new InvalidOperationException());
            }

            static readonly Material[] s_MaterialArray = new Material[1];

            public bool UseDefaultVisualElement => false;

            readonly ModifyMaterialNode _Node;
            readonly SerializedProperty _ShaderProperty;
            readonly SerializedProperty _PropertyValuesProperty;

            MaterialEditor m_MaterialEditor;
            Material m_Material;

            public ModifyMaterialNodeEditor(ModifyMaterialNode node, SerializedProperty processorProperty)
            {
                _Node = node;
                _ShaderProperty = processorProperty.FindPropertyRelative(nameof(m_Shader));
                _PropertyValuesProperty = processorProperty.FindPropertyRelative(nameof(m_PropertyValues));
            }

            public void OnEnable()
            {
                if (m_Material == null && _Node.m_Shader != null)
                {
                    m_Material = new Material(_Node.m_Shader) { hideFlags = HideFlags.DontSave };
                    foreach (var i in _Node.m_PropertyValues)
                        i?.SetPropertyValue(m_Material);
                }
                if (m_MaterialEditor == null)
                {
                    m_MaterialEditor = Editor.CreateEditor(m_Material, typeof(MaterialEditor)) as MaterialEditor;
                }
            }

            public void OnDisable()
            {
                if (m_Material != null)
                {
                    DestroyImmediate(m_Material);
                    m_Material = null;
                }
                if (m_MaterialEditor != null)
                {
                    DestroyImmediate(m_MaterialEditor);
                    m_MaterialEditor = null;
                }
            }

            public void OnGUI()
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_ShaderProperty);
                var changed = EditorGUI.EndChangeCheck();
                if ((m_Material == null || changed) && _ShaderProperty.objectReferenceValue is Shader shader)
                {
                    if (m_Material == null)
                        m_Material = new Material(shader);
                    else
                        m_Material.shader = shader;
                    _PropertyValuesProperty.arraySize = 0;
                }
                if (m_Material == null)
                    return;
                s_MaterialArray[0] = m_Material;
                var materialProperties = MaterialEditor.GetMaterialProperties(s_MaterialArray);
                _PropertyValuesProperty.arraySize = materialProperties.Length;
                var propertyValues = _Node.m_PropertyValues;
                EditorGUILayout.LabelField("Property Values", EditorStyles.boldLabel);
                EditorGUIUtility.labelWidth = 0;
                EditorGUIUtility.fieldWidth = 0;
                for (var i = 0; i < _PropertyValuesProperty.arraySize; ++i)
                {
                    using (var element = _PropertyValuesProperty.GetArrayElementAtIndex(i))
                    {
                        var materialProperty = materialProperties[i];
                        IPropertyValue propertyValue;
                        switch (materialProperty.type)
                        {
                            case MaterialProperty.PropType.Float:
                            case MaterialProperty.PropType.Range:
                                if (changed || !(i < propertyValues.Length && propertyValues[i] is FloatValue floatValue))
                                {
                                    floatValue = new FloatValue();
                                    if (Event.current.type != EventType.Layout)
                                        element.managedReferenceValue = floatValue;
                                }
                                propertyValue = floatValue;
                                break;
                            case MaterialProperty.PropType.Color:
                                if (changed || !(i < propertyValues.Length && propertyValues[i] is ColorValue colorValue))
                                {
                                    colorValue = new ColorValue();
                                    if (Event.current.type != EventType.Layout)
                                        element.managedReferenceValue = colorValue;
                                }
                                propertyValue = colorValue;
                                break;
                            case MaterialProperty.PropType.Vector:
                                if (changed || !(i < propertyValues.Length && propertyValues[i] is VectorValue vectorValue))
                                {
                                    vectorValue = new VectorValue();
                                    if (Event.current.type != EventType.Layout)
                                        element.managedReferenceValue = vectorValue;
                                }
                                propertyValue = vectorValue;
                                break;
                            case MaterialProperty.PropType.Texture:
                                if (changed || !(i < propertyValues.Length && propertyValues[i] is TextureValue textureValue))
                                {
                                    textureValue = new TextureValue();
                                    if (Event.current.type != EventType.Layout)
                                        element.managedReferenceValue = textureValue;
                                }
                                propertyValue = textureValue;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(materialProperty.type));
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.BeginChangeCheck();
                            var newEnabled = EditorGUILayout.Toggle(propertyValue.Enabled, GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight));
                            using (new EditorGUI.DisabledScope(!propertyValue.Enabled))
                            {
                                if (m_MaterialEditor == null)
                                    m_MaterialEditor = Editor.CreateEditor(m_Material, typeof(MaterialEditor)) as MaterialEditor;
                                m_MaterialEditor?.ShaderProperty(materialProperty, materialProperty.displayName);
                            }
                            if (EditorGUI.EndChangeCheck())
                            {
                                propertyValue.Enabled = newEnabled;
                                propertyValue.CopyFromMaterialProperty(materialProperty);
                                element.managedReferenceValue = propertyValue;
                            }
                        }
                    }
                }
            }
        }

        static ModifyMaterialNode()
        {
            INodeDataUtility.AddConstructor(() => new ModifyMaterialNode());
        }

        ModifyMaterialNode() { }

        [SerializeField]
        Shader m_Shader;
        [SerializeReference]
        IPropertyValue[] m_PropertyValues = Array.Empty<IPropertyValue>();

        ModifyMaterialNodeEditor m_Editor;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<Material>(isMulti: true);
            portDataContainer.AddOutputPort<Material>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            if (m_Shader != null)
            {
                foreach (var assets in assetGroup)
                {
                    foreach (var material in assets.GetAssetsFromType<Material>())
                    {
                        if (material.shader != m_Shader)
                            continue;
                        foreach (var i in m_PropertyValues)
                        {
                            if (i.Enabled)
                                i.SetPropertyValue(material);
                        }
                        if (EditorUtility.IsDirty(material))
                            EditorUtility.SetDirty(material);
                    }
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
