using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    static class UnityObjectTypeUtility
    {
        static readonly MethodInfo s_ScriptingWrapperTypeNameForNativeIDInfo = Type.GetType("UnityEditor.RuntimeClassMetadataUtils, UnityEditor.dll").GetMethod("ScriptingWrapperTypeNameForNativeID", BindingFlags.Static | BindingFlags.Public);

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

        static class ComponentCommand
        {
            static readonly Dictionary<string, int> menuPaths;
            static readonly Dictionary<string, string> commandToMenuPaths;

            static public string[] Commands { get; }
            static public string[] DisplayNames { get; }
            static public string[] DisplayNamesWithTransform { get; }
            static public IReadOnlyDictionary<string, int> MenuPaths => menuPaths;
            static public IReadOnlyDictionary<string, string> CommandToMenuPaths => commandToMenuPaths;

            static ComponentCommand()
            {
                var removeCount = "Component/".Length;
                var menus = Unsupported.GetSubmenus("Component");
                Commands = Unsupported.GetSubmenusCommands("Component");
                var dstMenus = new List<string>();
                menuPaths = new Dictionary<string, int>(menus.Length);
                commandToMenuPaths = new Dictionary<string, string>(menus.Length);
                for (var i = 0; i < menus.Length; ++i)
                {
                    if (Commands[i] != "ADD")
                    {
                        var dstPath = menus[i].Remove(0, removeCount);
                        menuPaths.Add(dstPath, menuPaths.Count);
                        dstMenus.Add(dstPath);
                        commandToMenuPaths.Add(Commands[i], dstPath);
                    }
                }
                DisplayNames = dstMenus.ToArray();
                dstMenus.Insert(0, "Transfrom");
                DisplayNamesWithTransform = dstMenus.ToArray();
            }
        }

        public static string ComponentTypePopup(string menuPath, bool includingTransform = false)
        {
            if (!ComponentCommand.MenuPaths.TryGetValue(menuPath, out var index))
                index = 0;
            if (includingTransform)
            {
                if (menuPath != ComponentCommand.DisplayNamesWithTransform[0])
                    ++index;
                index = EditorGUILayout.Popup(index, ComponentCommand.DisplayNamesWithTransform);
                return ComponentCommand.DisplayNamesWithTransform[index];
            }
            else
            {
                index = EditorGUILayout.Popup(index, ComponentCommand.DisplayNames);
                return ComponentCommand.DisplayNames[index];
            }
        }

        public static bool TryGetComponentTypeFromMenuPath(string menuPath, out Type componentType)
        {
            if (ComponentCommand.MenuPaths.TryGetValue(menuPath, out var index))
            {
                var command = ComponentCommand.Commands[index];
                if (command.StartsWith("SCRIPT"))
                {
                    var scriptId = int.Parse(command.Substring(6));
                    if (EditorUtility.InstanceIDToObject(scriptId) is MonoScript monoScript)
                        componentType = monoScript.GetClass();
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    var classId = int.Parse(command);
                    if (s_ScriptingWrapperTypeNameForNativeIDInfo.Invoke(null, new object[] { classId }) is string typeName)
                        componentType = Type.GetType($"{typeName}, UnityEngine.dll");
                    else
                        throw new InvalidOperationException();
                }
                return true;
            }
            else
            {
                componentType = typeof(Transform);
                return menuPath == ComponentCommand.DisplayNamesWithTransform[0];
            }
        }

        public static string GetMenuPath(MonoScript monoScript)
        {
            if (ComponentCommand.CommandToMenuPaths.TryGetValue($"SCRIPT{monoScript.GetInstanceID()}", out var menuPath))
                return menuPath;
            throw new InvalidOperationException();
        }

        public static string GetMenuPath(int classId)
        {
            if (ComponentCommand.CommandToMenuPaths.TryGetValue(classId.ToString(), out var menuPath))
                return menuPath;
            return ComponentCommand.DisplayNamesWithTransform[0];
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
