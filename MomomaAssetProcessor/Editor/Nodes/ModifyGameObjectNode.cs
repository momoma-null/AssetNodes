using System;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Modify/GameObject")]
    sealed class ModifyGameObjectNode : INodeProcessor, IPrefabModifier
    {
        ModifyGameObjectNode() { }

        enum PropertyType
        {
            Active,
            Tag,
            Layer,
            ContributeGI,
            OccluderStatic,
            BatchingStatic,
            NavigationStatic,
            OccludeeStatic,
            OffMeshLinkGeneration,
            ReflectionProbeStatic,
        }

        [Serializable]
        sealed class PropertySetting
        {
            [SerializeField]
            PropertyType m_PropertyType;
            [SerializeField]
            string m_Value = "";

            public PropertyType PropertyType => m_PropertyType;
            public string RawValue => m_Value;

            [CustomPropertyDrawer(typeof(PropertySetting))]
            sealed class PropertySettingDrawer : PropertyDrawer
            {
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    position.width *= 0.5f;
                    using (var propertyTypeProperty = property.FindPropertyRelative(nameof(m_PropertyType)))
                    using (var valueProperty = property.FindPropertyRelative(nameof(m_Value)))
                    {
                        EditorGUI.PropertyField(position, propertyTypeProperty, new GUIContent(""));
                        position.x += position.width;
                        switch ((PropertyType)propertyTypeProperty.enumValueIndex)
                        {
                            case PropertyType.Active:
                            case PropertyType.ContributeGI:
                            case PropertyType.OccluderStatic:
                            case PropertyType.BatchingStatic:
                            case PropertyType.NavigationStatic:
                            case PropertyType.OccludeeStatic:
                            case PropertyType.OffMeshLinkGeneration:
                            case PropertyType.ReflectionProbeStatic:
                                using (var check = new EditorGUI.ChangeCheckScope())
                                {
                                    if (!bool.TryParse(valueProperty.stringValue, out var boolValue))
                                        valueProperty.stringValue = default(bool).ToString();
                                    boolValue = EditorGUI.Toggle(position, boolValue);
                                    if (check.changed)
                                    {
                                        valueProperty.stringValue = boolValue.ToString();
                                    }
                                }
                                break;
                            case PropertyType.Tag:
                                using (var check = new EditorGUI.ChangeCheckScope())
                                {
                                    var newValue = EditorGUI.TagField(position, valueProperty.stringValue);
                                    if (check.changed)
                                    {
                                        valueProperty.stringValue = newValue;
                                    }
                                }
                                break;
                            case PropertyType.Layer:
                                using (var check = new EditorGUI.ChangeCheckScope())
                                {
                                    if (!int.TryParse(valueProperty.stringValue, out var intValue))
                                        valueProperty.stringValue = default(int).ToString();
                                    intValue = EditorGUI.LayerField(position, intValue);
                                    if (check.changed)
                                    {
                                        valueProperty.stringValue = intValue.ToString();
                                    }
                                }
                                break;
                            default: throw new ArgumentOutOfRangeException(nameof(m_PropertyType));
                        }
                    }
                }
            }
        }

        public bool IncludeChildren => m_IncludeChildren;
        public string RegexPattern => m_RegexPattern;

        [SerializeField]
        bool m_IncludeChildren = false;
        [SerializeField]
        string m_RegexPattern = string.Empty;
        [SerializeField]
        PropertySetting[] m_Properties = new PropertySetting[0];

        public Color HeaderColor => ColorDefinition.ModifyNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            this.ModifyPrefab(assetGroup);
            container.SetOutput(0, assetGroup);
        }

        public void Modify(GameObject go)
        {
            foreach (var setting in m_Properties)
            {
                switch (setting.PropertyType)
                {
                    case PropertyType.Active:
                        {
                            if (bool.TryParse(setting.RawValue, out var boolValue))
                            {
                                if (go.activeSelf != boolValue)
                                    go.SetActive(boolValue);
                            }
                            break;
                        }
                    case PropertyType.ContributeGI:
                    case PropertyType.OccluderStatic:
                    case PropertyType.BatchingStatic:
                    case PropertyType.NavigationStatic:
                    case PropertyType.OccludeeStatic:
                    case PropertyType.OffMeshLinkGeneration:
                    case PropertyType.ReflectionProbeStatic:
                        {
                            if (bool.TryParse(setting.RawValue, out var boolValue))
                            {
                                if (Enum.TryParse<StaticEditorFlags>(setting.PropertyType.ToString(), out var targetFlag))
                                {
                                    var flags = GameObjectUtility.GetStaticEditorFlags(go);
                                    if ((flags & targetFlag) > 0 != boolValue)
                                    {
                                        if (boolValue)
                                            flags |= targetFlag;
                                        else
                                            flags &= ~targetFlag;
                                        GameObjectUtility.SetStaticEditorFlags(go, flags);
                                    }
                                }
                            }
                            break;
                        }
                    case PropertyType.Tag:
                        go.tag = setting.RawValue;
                        break;
                    case PropertyType.Layer:
                        if (int.TryParse(setting.RawValue, out var intValue))
                            go.layer = intValue;
                        break;
                }
            }
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
