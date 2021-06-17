using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [Serializable]
    sealed class GameObjectModifierNode : INodeData, IFunctionNode
    {
        static GameObjectModifierNode()
        {
            INodeDataUtility.AddConstructor(() => new GameObjectModifierNode());
        }

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

        public string Title => "Modifiy GameObject";
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
        bool m_Recursively;

        [SerializeField]
        PropertySetting[] m_Properties = new PropertySetting[0];

        void IFunctionNode.Process(ProcessingDataContainer container)
        {
            var assets = container.Get(m_InputPort.Id, () => new AssetGroup());
            foreach (var asset in assets)
            {
                if (asset is GameObject target)
                {
                    foreach (var setting in m_Properties)
                    {
                        switch (setting.PropertyType)
                        {
                            case PropertyType.Active:
                                {
                                    bool.TryParse(setting.RawValue, out var boolValue);
                                    if (target.activeSelf != boolValue)
                                        target.SetActive(boolValue);
                                }
                                break;
                            case PropertyType.ContributeGI:
                            case PropertyType.OccluderStatic:
                            case PropertyType.BatchingStatic:
                            case PropertyType.NavigationStatic:
                            case PropertyType.OccludeeStatic:
                            case PropertyType.OffMeshLinkGeneration:
                            case PropertyType.ReflectionProbeStatic:
                                {
                                    bool.TryParse(setting.RawValue, out var boolValue);
                                    if (Enum.TryParse<StaticEditorFlags>(setting.PropertyType.ToString(), out var targetFlag))
                                    {
                                        var flags = GameObjectUtility.GetStaticEditorFlags(target);
                                        if ((flags & targetFlag) > 0 != boolValue)
                                        {
                                            if (boolValue)
                                                flags |= targetFlag;
                                            else
                                                flags &= ~targetFlag;
                                            GameObjectUtility.SetStaticEditorFlags(target, flags);
                                        }
                                    }
                                }
                                break;
                            case PropertyType.Tag:
                                target.tag = setting.RawValue;
                                break;
                            case PropertyType.Layer:
                                int.TryParse(setting.RawValue, out var intValue);
                                target.layer = intValue;
                                break;
                            default: throw new ArgumentOutOfRangeException(nameof(setting.PropertyType));
                        }
                    }
                }
            }
        }
    }
}
