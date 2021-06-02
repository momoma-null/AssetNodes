using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    sealed class GroupByTypeNode : INodeData
    {
        [Serializable]
        sealed class TypeGroup
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
            public static IList<Type> Types => s_Types.Values;

            public Type Type => s_Types.Values[index];

            public int index;

            public string regex = "";
        }

        [CustomPropertyDrawer(typeof(TypeGroup))]
        sealed class TypeGroupDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                property.serializedObject.UpdateIfRequiredOrScript();
                position.width *= 0.5f;
                EditorGUI.BeginChangeCheck();
                using (var regexProperty = property.FindPropertyRelative(nameof(TypeGroup.regex)))
                    regexProperty.stringValue = EditorGUI.TextField(position, regexProperty.stringValue);
                position.x += position.width;
                using (var indexProperty = property.FindPropertyRelative(nameof(TypeGroup.index)))
                    indexProperty.intValue = EditorGUI.Popup(position, indexProperty.intValue, TypeGroup.TypeNames.ToArray());
                if (EditorGUI.EndChangeCheck())
                    property.serializedObject.ApplyModifiedProperties();
            }
        }

        static GroupByTypeNode()
        {
            INodeDataUtility.AddConstructor(() => new GroupByTypeNode());
        }

        public string Title => "Group by Type";
        public string MenuPath => "Group/Group by Type";
        public IEnumerable<PortData> InputPorts => new[] { new PortData(typeof(UnityObject)) };
        public IEnumerable<PortData> OutputPorts => m_TypeGroups.Select(i => new PortData(i.Type));

        [SerializeField]
        List<TypeGroup> m_TypeGroups = new List<TypeGroup>();

        public IEnumerable<PropertyValue> GetProperties()
        {
            yield return new PropertyValueList(nameof(m_TypeGroups), m_TypeGroups);
        }
    }
}
