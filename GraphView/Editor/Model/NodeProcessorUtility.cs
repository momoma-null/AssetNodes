using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    public static class NodeProcessorUtility
    {
        public static IDictionary<string, Func<INodeProcessor>> GetConstructors(Type graphType)
        {
            var types = TypeCache.GetTypesWithAttribute<CreateElementAttribute>();
            var ctors = new Dictionary<string, Func<INodeProcessor>>();
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach(var type in types)
            {
                var attr = type.GetCustomAttribute<CreateElementAttribute>();
                if (attr.GraphType == graphType)
                {
                    var constructorInfo = type.GetConstructor(bindingFlags, Type.DefaultBinder,  Array.Empty<Type>(), null);
                    ctors.Add(attr.MenuPath, () => (INodeProcessor)constructorInfo.Invoke(Array.Empty<object>()));
                }
            }
            return ctors;
        }
    }
}
