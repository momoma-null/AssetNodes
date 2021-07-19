using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    static class UnityObjectTypeUtility
    {
        sealed class AssetTypeDatas : Dictionary<string, AssetTypeData>
        {
            Dictionary<string, int> indexCache = new Dictionary<string, int>();
            string[] keyArray = new string[0];

            public string this[int index] => keyArray[index];
            public void Add(AssetTypeData assetTypeData) => Add(assetTypeData.AssetType.AssemblyQualifiedName, assetTypeData);
            public bool TryGetIndex(string key, out int index) => indexCache.TryGetValue(key, out index);

            public AssetTypeDatas Initialize()
            {
                keyArray = Keys.ToArray();
                indexCache = new Dictionary<string, int>(keyArray.Length);
                for (var i = 0; i < keyArray.Length; ++i)
                    indexCache[keyArray[i]] = i;
                return this;
            }
        }

        static readonly AssetTypeDatas s_Types = new AssetTypeDatas() {
                AssetTypeData.Create<UnityObject>("Any"),
                AssetTypeData.Create<AnimationClip>("AnimationClip"),
                AssetTypeData.Create<AudioClip, AudioImporter>("AudioClip"),
                AssetTypeData.Create<UnityEngine.Audio.AudioMixer>("AudioMixer"),
                AssetTypeData.Create<ComputeShader, ComputeShaderImporter>("ComputeShader"),
                AssetTypeData.Create<Font, TrueTypeFontImporter>("Font"),
                AssetTypeData.Create<GUISkin>("GUISkin"),
                AssetTypeData.Create<Material>("Material"),
                AssetTypeData.Create<Mesh>("Mesh"),
                AssetTypeData.Create<PhysicMaterial>("PhysicMaterial"),
                AssetTypeData.Create<GameObject>("Prefab"),
                AssetTypeData.Create<SceneAsset>("Scene"),
                AssetTypeData.Create<MonoScript, MonoImporter>("Script"),
                AssetTypeData.Create<Shader, ShaderImporter>("Shader"),
                AssetTypeData.Create<Texture, TextureImporter>("Texture"),
                AssetTypeData.Create<UnityEngine.Video.VideoClip, VideoClipImporter>("VideoClip"), }.Initialize();

        public static string[] TypeNames { get; } = s_Types.Values.Select(i => i.DisplayName).ToArray();

        public static AssetTypeData GetAssetTypeData(string type) => s_Types[type];

        public static string AssetTypePopup(string type)
        {
            if (!s_Types.TryGetIndex(type, out var index))
                index = 0;
            index = EditorGUILayout.Popup(index, TypeNames);
            return 0 <= index && index < s_Types.Count ? s_Types[index] : "";
        }

        public static string AssetTypePopup(Rect position, string type)
        {
            if (!s_Types.TryGetIndex(type, out var index))
                index = 0;
            index = EditorGUI.Popup(position, index, TypeNames);
            return 0 <= index && index < s_Types.Count ? s_Types[index] : "";
        }
    }

    public sealed class AssetTypeData
    {
        public static AssetTypeData Create<T>(string name) => new AssetTypeData(i => i is T, typeof(T), name);
        public static AssetTypeData Create<T1, T2>(string name) => new AssetTypeData(i => i is T1 || i is T2, typeof(T1), name);

        AssetTypeData(Func<UnityObject, bool> isTarget, Type targetType, string displayName)
        {
            AssetType = targetType;
            DisplayName = displayName;
            this.isTarget = isTarget;
        }

        readonly Func<UnityObject, bool> isTarget;

        public Type AssetType { get; }
        public string DisplayName { get; }
        public bool IsTarget(UnityObject x) => isTarget(x);
    }

}
