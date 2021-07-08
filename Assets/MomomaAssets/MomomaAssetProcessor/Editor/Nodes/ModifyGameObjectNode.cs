using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [Serializable]
    sealed class ModifyGameObjectNode : INodeData
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
                                    bool.TryParse(valueProperty.stringValue, out var boolValue);
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
                                    int.TryParse(valueProperty.stringValue, out var intValue);
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

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();
        public string MenuPath => "Modify/GameObject";
        public IEnumerable<PortData> InputPorts => new[] { m_InputPort };
        public IEnumerable<PortData> OutputPorts => new[] { m_OutputPort };

        [SerializeField]
        [HideInInspector]
        PortData m_InputPort = new PortData(typeof(GameObject));

        [SerializeField]
        [HideInInspector]
        PortData m_OutputPort = new PortData(typeof(GameObject));

        [SerializeField]
        PropertySetting[] m_Properties = new PropertySetting[0];

        public void Process(ProcessingDataContainer container)
        {
            var assetGroup = container.Get(m_InputPort.Id, () => new AssetGroup());
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
                foreach (var asset in assetGroup)
                {
                    foreach (var go in asset.GetAssetsFromType<GameObject>())
                    {
                        process(go);
                    }
                }
            }
            container.Set(m_OutputPort.Id, assetGroup);
        }
    }
}
