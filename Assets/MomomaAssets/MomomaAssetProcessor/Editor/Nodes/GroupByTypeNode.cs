using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    sealed class GroupByTypeNode : INodeData, ISerializationCallbackReceiver
    {
        sealed class AssetTypeData
        {
            public static AssetTypeData Create<T>() => new AssetTypeData(i => i is T, typeof(T));
            public static AssetTypeData Create<T1, T2>() => new AssetTypeData(i => i is T1 || i is T2, typeof(T1));

            AssetTypeData(Func<UnityObject, bool> isTarget, Type targetType)
            {
                AssetType = targetType;
                m_IsTarget = isTarget;
            }

            readonly Func<UnityObject, bool> m_IsTarget;

            public Type AssetType { get; }

            public bool IsTarget(UnityObject x) => m_IsTarget(x);
        }

        [Serializable]
        sealed class TypeGroup : ISerializationCallbackReceiver
        {
            static readonly SortedList<string, AssetTypeData> s_Types = new SortedList<string, AssetTypeData>() {
                { "AnimationClip", AssetTypeData.Create<AnimationClip>() },
                { "AudioClip", AssetTypeData.Create<AudioClip, AudioImporter>() },
                { "AudioMixer", AssetTypeData.Create<UnityEngine.Audio.AudioMixer>() },
                { "ComputeShader", AssetTypeData.Create<ComputeShader, ComputeShaderImporter>() },
                { "Font", AssetTypeData.Create<Font, TrueTypeFontImporter>() },
                { "GUISkin", AssetTypeData.Create<GUISkin>() },
                { "Material", AssetTypeData.Create<Material>() },
                { "Mesh", AssetTypeData.Create<Mesh>() },
                { "PhysicMaterial", AssetTypeData.Create<PhysicMaterial>() },
                { "Prefab", AssetTypeData.Create<GameObject>() },
                { "Scene", AssetTypeData.Create<SceneAsset>() },
                { "Script", AssetTypeData.Create<MonoScript, MonoImporter>() },
                { "Shader", AssetTypeData.Create<Shader, ShaderImporter>() },
                { "Texture", AssetTypeData.Create<Texture, TextureImporter>() },
                { "VideoClip", AssetTypeData.Create<UnityEngine.Video.VideoClip, VideoClipImporter>() }, };

            public static IList<string> TypeNames => s_Types.Keys;

            public AssetTypeData AssetTypeData => s_Types.Values[index];

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
                    indexProperty.intValue = EditorGUI.Popup(position, indexProperty.intValue, TypeGroup.TypeNames.ToArray());
            }
        }

        static GroupByTypeNode()
        {
            INodeDataUtility.AddConstructor(() => new GroupByTypeNode());
        }

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();
        public string MenuPath => "Group/Group by Type";
        public IEnumerable<PortData> InputPorts => m_InputPorts;
        public IEnumerable<PortData> OutputPorts => m_TypeGroups.Select(i => new PortData(i.AssetTypeData.AssetType, id: i.guid));

        [SerializeField]
        [HideInInspector]
        PortData[] m_InputPorts = new[] { new PortData(typeof(UnityObject)) };

        [SerializeField]
        List<TypeGroup> m_TypeGroups = new List<TypeGroup>();

        public void Process(ProcessingDataContainer container)
        {
            var objects = container.Get(m_InputPorts[0].Id, () => new AssetGroup());
            foreach (var typeGroup in m_TypeGroups)
            {
                var result = new AssetGroup();
                var regex = new Regex(typeGroup.regex);
                foreach (var i in objects)
                {
                    var path = AssetDatabase.GetAssetPath(i);
                    var importer = AssetImporter.GetAtPath(path);
                    if ((importer != null && typeGroup.AssetTypeData.IsTarget(importer)) || typeGroup.AssetTypeData.IsTarget(i))
                    {
                        if (string.IsNullOrEmpty(typeGroup.regex) || regex.Match(path).Success)
                        {
                            result.Add(i);
                        }
                    }
                }
                container.Set(typeGroup.guid, result);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_TypeGroups.Count > 1)
            {
                if (m_TypeGroups[m_TypeGroups.Count - 1].guid == m_TypeGroups[m_TypeGroups.Count - 2].guid)
                    m_TypeGroups[m_TypeGroups.Count - 1].guid = Guid.NewGuid().ToString();
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
