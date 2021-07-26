using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Object;

#nullable enable

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
            public bool enabled;
            public string propertyName;
            public float floatValue;

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
            public bool enabled;
            public string propertyName;
            public Color colorValue;

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
            public bool enabled;
            public string propertyName;
            public Vector4 vectorValue;

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
            public bool enabled;
            public string propertyName;
            public Texture objectReferenceValue;
            public Vector2 scale;
            public Vector2 offset;

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
            public bool UseDefaultVisualElement => false;

            MaterialEditor? m_MaterialEditor;
            Material? m_Material;
            MaterialProperty[] m_MaterialProperties = new MaterialProperty[0];
            Func<IPropertyValue[]> getPropertyValues;

            public ModifyMaterialNodeEditor(Func<IPropertyValue[]> getPropertyValues)
            {
                this.getPropertyValues = getPropertyValues;
            }

            public void OnDestroy()
            {
                if (m_Material != null)
                    DestroyImmediate(m_Material);
                if (m_MaterialEditor != null)
                    DestroyImmediate(m_MaterialEditor);
                m_MaterialProperties = new MaterialProperty[0];
            }

            public void OnGUI(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty)
            {
                using (var m_ShaderProperty = processorProperty.FindPropertyRelative(nameof(m_Shader)))
                using (var m_PropertyValuesProperty = processorProperty.FindPropertyRelative(nameof(m_PropertyValues)))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(m_ShaderProperty);
                    var changed = EditorGUI.EndChangeCheck();
                    if (changed)
                    {
                        m_PropertyValuesProperty.arraySize = 0;
                    }
                    if (m_ShaderProperty.objectReferenceValue is Shader shader)
                    {
                        var propertyValues = getPropertyValues();
                        if (m_Material == null)
                        {
                            m_Material = new Material(shader) { hideFlags = HideFlags.DontSave };
                            foreach (var i in propertyValues)
                            {
                                i.SetPropertyValue(m_Material);
                            }
                            m_MaterialProperties = MaterialEditor.GetMaterialProperties(new[] { m_Material });
                        }
                        if (m_MaterialEditor == null)
                        {
                            m_MaterialEditor = Editor.CreateEditor(m_Material, typeof(MaterialEditor)) as MaterialEditor;
                            if (m_MaterialEditor == null)
                                throw new NullReferenceException();
                        }
                        if (changed)
                        {
                            m_Material.shader = shader;
                            m_MaterialProperties = MaterialEditor.GetMaterialProperties(new[] { m_Material });
                            m_PropertyValuesProperty.arraySize = m_MaterialProperties.Length;
                        }
                        EditorGUILayout.LabelField("Property Values", EditorStyles.boldLabel);
                        using (new EditorGUI.IndentLevelScope(1))
                        {
                            EditorGUIUtility.labelWidth = 0;
                            EditorGUIUtility.fieldWidth = 0;
                            for (var i = 0; i < m_PropertyValuesProperty.arraySize; ++i)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                using (var element = m_PropertyValuesProperty.GetArrayElementAtIndex(i))
                                {
                                    var materialProperty = m_MaterialProperties[i];
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
                                    using (var enableProperty = element.FindPropertyRelative(nameof(FloatValue.enabled)))
                                    {
                                        if (enableProperty == null)
                                            break;
                                        EditorGUI.BeginChangeCheck();
                                        var newEnabled = EditorGUILayout.Toggle(enableProperty.boolValue, GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight));
                                        using (new EditorGUI.DisabledScope(!enableProperty.boolValue))
                                        {
                                            m_MaterialEditor.ShaderProperty(materialProperty, materialProperty.displayName);
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
                }
            }
        }

        static ModifyMaterialNode()
        {
            INodeDataUtility.AddConstructor(() => new ModifyMaterialNode());
        }

        ModifyMaterialNode() { }

        [SerializeField]
        Shader? m_Shader;
        [SerializeReference]
        IPropertyValue[] m_PropertyValues = new IPropertyValue[0];

        ModifyMaterialNodeEditor? m_Editor;

        public INodeProcessorEditor ProcessorEditor => m_Editor ?? (m_Editor = new ModifyMaterialNodeEditor(GetPropertyValues));

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(Material)));
            portDataContainer.OutputPorts.Add(new PortData(typeof(Material)));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], this.NewAssetGroup);
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

        IPropertyValue[] GetPropertyValues() => m_PropertyValues;
    }
}
