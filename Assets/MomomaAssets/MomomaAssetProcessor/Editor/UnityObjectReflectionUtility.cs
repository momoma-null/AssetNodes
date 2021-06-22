using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    static class UnityObjectReflectionUtility
    {
        static readonly Dictionary<Type, PropertyInfo[]> s_PropertyInfos = new Dictionary<Type, PropertyInfo[]>();

        public static IReadOnlyList<PropertyInfo> GetPropertyInfos(Type type)
        {
            if (!s_PropertyInfos.TryGetValue(type, out var infos))
            {
                infos = type.GetProperties();
                s_PropertyInfos.Add(type, infos);
            }
            return infos;
        }

        static readonly SortedSet<Type> s_UnityObjectTypes = new SortedSet<Type>(
                                                                        AppDomain.CurrentDomain.GetAssemblies().
                                                                        SelectMany(i => i.GetTypes()).
                                                                        Where(i => i.IsSubclassOf(typeof(UnityEngine.Object))));

        static readonly string[] s_TypeNames = s_UnityObjectTypes.Select(i => i.FullName.Replace('.', '/')).ToArray();

        public static int TypePopupLayout(int selected)
        {
            return EditorGUILayout.Popup(selected, s_TypeNames);
        }
    }
}
