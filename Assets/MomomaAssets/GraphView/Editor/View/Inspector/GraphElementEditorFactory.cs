using System;
using System.Collections.Generic;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    delegate BaseGraphElementEditor GenerateGraphElementEditor(IGraphElementData graphElementData, SerializedProperty property);

    static class GraphElementEditorFactory
    {
        sealed class EntryEditorFactory : IEntryDelegate<GenerateGraphElementEditor>
        {
            public readonly Dictionary<Type, GenerateGraphElementEditor> factories;

            public EntryEditorFactory(int capacity = 0)
            {
                factories = new Dictionary<Type, GenerateGraphElementEditor>(capacity);
            }

            public void Add(Type type, GenerateGraphElementEditor function)
            {
                factories.Add(type, function);
            }
        }

        static readonly IReadOnlyDictionary<Type, GenerateGraphElementEditor> s_Factories;

        static GraphElementEditorFactory()
        {
            var methods = TypeCache.GetMethodsWithAttribute<GraphElementEditorFactoryAttribute>();
            var factories = new EntryEditorFactory(methods.Count);
            var parameters = new object[] { factories };
            foreach (var methodInfo in methods)
                methodInfo.Invoke(null, parameters);
            s_Factories = factories.factories;
        }

        public static BaseGraphElementEditor GetEditor(IGraphElementData graphElementData, SerializedProperty property)
        {
            if (s_Factories.TryGetValue(graphElementData.GetType(), out var factory))
            {
                return factory(graphElementData, property);
            }
            return new DefaultGraphElementEditor(property);
        }
    }
}
