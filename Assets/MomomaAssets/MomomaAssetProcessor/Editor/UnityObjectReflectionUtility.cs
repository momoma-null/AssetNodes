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
    static class ComponentReflectionUtility
    {
        sealed class PropertiesInfo
        {
            public string[] Names { get; }
            public SortedList<string, PropertyInfo> Infos { get; }

            public PropertiesInfo(Type type)
            {
                Infos = new SortedList<string, PropertyInfo>(type.GetProperties().Where(i => i.CanRead && i.CanWrite).ToDictionary(i => i.Name, i => i));
                Names = Infos.Keys.ToArray();
            }
        }

        static readonly Dictionary<Type, PropertiesInfo> s_PropertiesInfos = new Dictionary<Type, PropertiesInfo>();

        static PropertiesInfo GetPropertiesInfo(Type type)
        {
            if (!s_PropertiesInfos.TryGetValue(type, out var info))
            {
                info = new PropertiesInfo(type);
                s_PropertiesInfos.Add(type, info);
            }
            return info;
        }

        public static string PropertyPopup(Rect position, string typeName, string propertyName)
        {
            if (s_ComponentTypes.TryGetValue(typeName, out var type))
            {
                var info = GetPropertiesInfo(type);
                var index = EditorGUI.Popup(position, info.Infos.IndexOfKey(propertyName), info.Names);
                index = Mathf.Clamp(index, 0, info.Names.Length);
                return info.Names[index];
            }
            return "";
        }

        static readonly HashSet<Type> s_CantAddComponents = new HashSet<Type>() { typeof(Component), typeof(Behaviour), typeof(MonoBehaviour), typeof(Joint), typeof(Joint2D), typeof(Collider), typeof(Collider2D) };

        static readonly SortedList<string, Type> s_ComponentTypes = new SortedList<string, Type>(
                                                                        AppDomain.CurrentDomain.GetAssemblies().
                                                                        SelectMany(i => i.GetTypes()).
                                                                        Where(i => i.IsSubclassOf(typeof(Component)) && !i.IsAbstract && !s_CantAddComponents.Contains(i)).
                                                                        ToDictionary(i => i.FullName.Replace('.', '/'), i => i));
        static readonly string[] s_TypeNames = s_ComponentTypes.Keys.ToArray();

        public static string TypePopup(Rect position, string typeName)
        {
            var index = s_ComponentTypes.IndexOfKey(typeName);
            index = EditorGUI.Popup(position, index, s_TypeNames);
            index = Mathf.Clamp(index, 0, s_TypeNames.Length);
            return s_TypeNames[index];
        }

        static readonly GameObject s_DefaultPrefab = Resources.Load<GameObject>("DefaultPrefab");
        static readonly GameObject s_VariantPrefab = Resources.Load<GameObject>("VariantPrefab");
        static readonly string s_DefaultPrefabPath = AssetDatabase.GetAssetPath(s_DefaultPrefab);
        static readonly Dictionary<Type, Component> s_InstanceComponents = new Dictionary<Type, Component>();

        public static void DeserializeVariantPrefab(string data)
        {
            EditorJsonUtility.FromJsonOverwrite(data, PrefabUtility.GetPrefabInstanceHandle(s_VariantPrefab));
        }

        public static string SerializeVariantPrefab()
        {
            return EditorJsonUtility.ToJson(PrefabUtility.GetPrefabInstanceHandle(s_VariantPrefab));
        }

        public static string GetInitialVariantPrefab()
        {
            PrefabUtility.RevertPrefabInstance(s_VariantPrefab, InteractionMode.AutomatedAction);
            return SerializeVariantPrefab();
        }

        public static Component? GetInstanceComponent(string typeName)
        {
            if (s_ComponentTypes.TryGetValue(typeName, out var type))
            {
                if (!s_InstanceComponents.TryGetValue(type, out var component))
                {
                    var tf = s_VariantPrefab.transform.Find(typeName);
                    if (tf == null)
                    {
                        var names = typeName.Split('/');
                        var prefab = PrefabUtility.LoadPrefabContents(s_DefaultPrefabPath);
                        try
                        {
                            var parent = prefab.transform;
                            foreach (var i in names)
                            {
                                tf = parent.Find(i);
                                if (tf == null)
                                    tf = new GameObject(i).transform;
                                tf.SetParent(parent);
                                parent = tf;
                            }
                            tf!.gameObject.AddComponent(type);
                            PrefabUtility.SaveAsPrefabAsset(prefab, s_DefaultPrefabPath);
                        }
                        finally
                        {
                            PrefabUtility.UnloadPrefabContents(prefab);
                        }
                        tf = s_VariantPrefab.transform.Find(typeName);
                    }
                    component = tf.GetComponent(type);
                    if (component == null)
                    {
                        component = tf.gameObject.AddComponent(type);
                        PrefabUtility.ApplyAddedComponent(component, s_DefaultPrefabPath, InteractionMode.AutomatedAction);
                    }
                    s_InstanceComponents.Add(type, component);
                }
                return component;
            }
            return null;
        }
    }
}
