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
    [InitializeOnLoad]
    [CreateElement("Group/Group by Type")]
    sealed class GroupByTypeNode : INodeProcessor, ISerializationCallbackReceiver
    {
        [Serializable]
        sealed class TypeGroup : ISerializationCallbackReceiver
        {
            public AssetTypeData AssetTypeData => UnityObjectTypeUtility.GetAssetTypeData(index);

            public int index;
            public string regex = "";
            public string guid = "";

            void ISerializationCallbackReceiver.OnAfterDeserialize()
            {
                if (string.IsNullOrEmpty(guid))
                    guid = Guid.NewGuid().ToString();
            }

            void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        }

        [CustomPropertyDrawer(typeof(TypeGroup))]
        sealed class TypeGroupDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                position.width *= 0.5f;
                using (var regexProperty = property.FindPropertyRelative(nameof(TypeGroup.regex)))
                    regexProperty.stringValue = EditorGUI.TextField(position, regexProperty.stringValue);
                position.x += position.width;
                using (var indexProperty = property.FindPropertyRelative(nameof(TypeGroup.index)))
                    indexProperty.intValue = EditorGUI.Popup(position, indexProperty.intValue, UnityObjectTypeUtility.TypeNames);
            }
        }

        static GroupByTypeNode()
        {
            INodeDataUtility.AddConstructor(() => new GroupByTypeNode());
        }

        GroupByTypeNode() { }

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();
        public IEnumerable<PortData> InputPorts => m_InputPorts;
        public IEnumerable<PortData> OutputPorts => m_TypeGroups.Select(i => new PortData(i.AssetTypeData.AssetType, id: i.guid));

        [SerializeField]
        [HideInInspector]
        PortData[] m_InputPorts = new[] { new PortData(typeof(UnityObject)) };

        [SerializeField]
        List<TypeGroup> m_TypeGroups = new List<TypeGroup>();

        public void Process(ProcessingDataContainer container)
        {
            var assetGroup = new AssetGroup(container.Get(m_InputPorts[0].Id, () => new AssetGroup()));
            foreach (var typeGroup in m_TypeGroups)
            {
                var result = new AssetGroup();
                var regex = new Regex(typeGroup.regex);
                foreach (var assets in assetGroup)
                {
                    var path = assets.AssetPath;
                    var importer = AssetImporter.GetAtPath(path);
                    if ((importer != null && typeGroup.AssetTypeData.IsTarget(importer)) || typeGroup.AssetTypeData.IsTarget(assets.MainAsset))
                    {
                        if (string.IsNullOrEmpty(typeGroup.regex) || regex.Match(path).Success)
                        {
                            result.Add(assets);
                        }
                    }
                }
                assetGroup.ExceptWith(result);
                container.Set(typeGroup.guid, result);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_TypeGroups.Count > 1)
            {
                var guids = new HashSet<string>();
                foreach (var i in m_TypeGroups)
                {
                    if (!guids.Add(i.guid))
                        i.guid = Guid.NewGuid().ToString();
                }
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
