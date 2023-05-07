using System;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    delegate IGraphElementEditor CreateGraphElementEditor<T>(T graphElementData, SerializedProperty property) where T : IGraphElementData;

    static class GraphElementEditorFactory
    {
        static class CachedFactory<T> where T : IGraphElementData
        {
            public static CreateGraphElementEditor<T>? factory;
        }

        sealed class FunctionProxy : IFunctionContainer<IGraphElementData, IGraphElementEditor>
        {
            SerializedProperty _property;

            public FunctionProxy(SerializedProperty property)
            {
                _property = property;
            }

            public IGraphElementEditor DoFunction<T>(T arg) where T : IGraphElementData
            {
                return CachedFactory<T>.factory?.Invoke(arg, _property) ?? new DefaultGraphElementEditor(_property);
            }
        }

        static GraphElementEditorFactory()
        {
            var methods = TypeCache.GetMethodsWithAttribute<GraphElementEditorFactoryAttribute>();
            foreach (var methodInfo in methods)
                methodInfo.Invoke(null, Array.Empty<object>());
        }

        public static void EntryEditorFactory<T>(CreateGraphElementEditor<T> createGraphElementEditor) where T : IGraphElementData
        {
            CachedFactory<T>.factory = createGraphElementEditor;
        }

        public static IGraphElementEditor CreateEditor(IGraphElementData graphElementData, SerializedProperty property)
        {
            var functionProxy = new FunctionProxy(property);
            return graphElementData.DoFunction(functionProxy);
        }
    }
}
