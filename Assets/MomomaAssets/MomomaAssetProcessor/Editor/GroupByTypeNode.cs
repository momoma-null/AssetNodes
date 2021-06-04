using System;
using System.Linq;
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
        [Serializable]
        sealed class TypeGroup : ISerializationCallbackReceiver
        {
            static readonly SortedList<string, Type> s_Types = new SortedList<string, Type>() {
                { "AnimationClip", typeof(AnimationClip) },
                { "AudioClip", typeof(AudioClip) },
                { "AudioMixer", typeof(UnityEngine.Audio.AudioMixer) },
                { "ComputeShader", typeof(ComputeShader) },
                { "Font", typeof(Font) },
                { "GUISkin", typeof(GUISkin) },
                { "Material", typeof(Material) },
                { "Mesh", typeof(Mesh) },
                { "Model", typeof(GameObject) },
                { "PhysicMaterial", typeof(PhysicMaterial) },
                { "Prefab", typeof(GameObject) },
                { "Scene", typeof(SceneAsset) },
                { "Script", typeof(MonoScript) },
                { "Shader", typeof(Shader) },
                { "Sprite", typeof(Sprite) },
                { "Texture", typeof(Texture) },
                { "VideoClip", typeof(UnityEngine.Video.VideoClip) }, };

            public static IList<string> TypeNames => s_Types.Keys;

            public Type Type => s_Types.Values[index];

            public int index;
            public string regex = "";
            public string guid = "";

            TypeGroup() { }

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

        public string Title => "Group by Type";
        public string MenuPath => "Group/Group by Type";
        public IEnumerable<PortData> InputPorts => m_InputPorts;
        public IEnumerable<PortData> OutputPorts => m_TypeGroups.Select(i => new PortData(i.Type, id: i.guid));

        PortData[] m_InputPorts = new[] { new PortData(typeof(UnityObject)) };

        [SerializeField]
        List<TypeGroup> m_TypeGroups = new List<TypeGroup>();

        public IEnumerable<PropertyValue> GetProperties()
        {
            yield return new PropertyValueList(nameof(m_TypeGroups), m_TypeGroups);
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
