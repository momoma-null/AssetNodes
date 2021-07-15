using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Group/Group by Type")]
    sealed class GroupByTypeNode : INodeProcessor, ISerializationCallbackReceiver
    {
        [Serializable]
        sealed class TypeGroup
        {
            public AssetTypeData AssetTypeData => UnityObjectTypeUtility.GetAssetTypeData(m_PortData.PortTypeName);

            [SerializeField]
            PortData m_PortData = new PortData(typeof(UnityObject));
            [SerializeField]
            string m_Regex = "";

            public PortData PortData => m_PortData;
            public string Regex => m_Regex;

            [CustomPropertyDrawer(typeof(TypeGroup))]
            sealed class TypeGroupDrawer : PropertyDrawer
            {
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    position.width *= 0.5f;
                    using (var regexProperty = property.FindPropertyRelative(nameof(m_Regex)))
                        regexProperty.stringValue = EditorGUI.TextField(position, regexProperty.stringValue);
                    position.x += position.width;
                    using (var portTypeProperty = property.FindPropertyRelative("m_PortData.m_PortType"))
                    {
                        EditorGUI.BeginChangeCheck();
                        var newValue = UnityObjectTypeUtility.AssetTypePopup(position, portTypeProperty.stringValue);
                        if (EditorGUI.EndChangeCheck())
                            portTypeProperty.stringValue = newValue;
                    }
                }
            }
        }

        static GroupByTypeNode()
        {
            INodeDataUtility.AddConstructor(() => new GroupByTypeNode());
        }

        GroupByTypeNode() { }

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();
        public IEnumerable<PortData> InputPorts => new[] { m_InputPort };
        public IEnumerable<PortData> OutputPorts => m_TypeGroups.Select(i => i.PortData);

        [SerializeField]
        [HideInInspector]
        PortData m_InputPort = new PortData(typeof(UnityObject));

        [SerializeField]
        TypeGroup[] m_TypeGroups = new TypeGroup[0];

        public void Process(ProcessingDataContainer container)
        {
            var assetGroup = new AssetGroup(container.Get(m_InputPort, this.NewAssetGroup));
            foreach (var typeGroup in m_TypeGroups)
            {
                var result = new AssetGroup();
                var regex = new Regex(typeGroup.Regex);
                foreach (var assets in assetGroup)
                {
                    var path = assets.AssetPath;
                    var importer = AssetImporter.GetAtPath(path);
                    if ((importer != null && typeGroup.AssetTypeData.IsTarget(importer)) || typeGroup.AssetTypeData.IsTarget(assets.MainAsset))
                    {
                        if (string.IsNullOrEmpty(typeGroup.Regex) || regex.Match(path).Success)
                        {
                            result.Add(assets);
                        }
                    }
                }
                assetGroup.ExceptWith(result);
                container.Set(typeGroup.PortData, result);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_TypeGroups.Length > 1)
            {
                var guids = new HashSet<string>();
                foreach (var i in m_TypeGroups)
                {
                    if (!guids.Add(i.PortData.Id))
                        i.PortData.Id = PortData.GetNewId();
                }
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
