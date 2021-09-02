using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Modify/GameObject")]
    sealed class ModifyGameObjectNode : INodeProcessor
    {
        static ModifyGameObjectNode()
        {
            INodeDataUtility.AddConstructor(() => new ModifyGameObjectNode());
        }

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

        [SerializeField]
        bool m_IncludeChildren = false;
        [SerializeField]
        PropertySetting[] m_Properties = new PropertySetting[0];

        public INodeProcessorEditor ProcessorEditor { get; } = new DefaultNodeProcessorEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(GameObject)));
            portDataContainer.OutputPorts.Add(new PortData(typeof(GameObject)));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], this.NewAssetGroup, this.CopyAssetGroup);
            Action<GameObject>? process = null;
            foreach (var setting in m_Properties)
            {
                switch (setting.PropertyType)
                {
                    case PropertyType.Active:
                        {
                            if (bool.TryParse(setting.RawValue, out var boolValue))
                            {
                                process += go =>
                                {
                                    if (go.activeSelf != boolValue)
                                        go.SetActive(boolValue);
                                };
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
                                    process += go =>
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
                                    };
                                }
                            }
                            break;
                        }
                    case PropertyType.Tag:
                        process += go => go.tag = setting.RawValue;
                        break;
                    case PropertyType.Layer:
                        if (int.TryParse(setting.RawValue, out var intValue))
                            process += go => go.layer = intValue;
                        break;
                }
            }
            if (process != null)
            {
                foreach (var assets in assetGroup)
                {
                    if (m_IncludeChildren)
                        foreach (var go in assets.GetAssetsFromType<GameObject>())
                            process(go);
                    else if (assets.MainAsset is GameObject root)
                        process(root);
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
