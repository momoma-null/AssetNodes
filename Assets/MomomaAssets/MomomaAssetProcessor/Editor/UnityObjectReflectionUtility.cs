using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

        public static string TypePopup(string typeName)
        {
            var index = s_ComponentTypes.IndexOfKey(typeName);
            index = EditorGUILayout.Popup(index, s_TypeNames);
            index = Mathf.Clamp(index, 0, s_TypeNames.Length);
            return s_TypeNames[index];
        }

        public static bool TryGetType(string typeName, out Type type) => s_ComponentTypes.TryGetValue(typeName, out type);
    }
}
