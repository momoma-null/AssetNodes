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
        static readonly List<AssetTypeData> s_Types = new List<AssetTypeData>() {
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
                AssetTypeData.Create<UnityEngine.Video.VideoClip, VideoClipImporter>("VideoClip"), };

        public static string[] TypeNames { get; } = s_Types.Select(i => i.DisplayName).ToArray();

        public static AssetTypeData GetAssetTypeData(int index) => s_Types[index];
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
