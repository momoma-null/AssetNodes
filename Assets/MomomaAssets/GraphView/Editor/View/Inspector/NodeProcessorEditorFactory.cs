using System;
using System.Collections.Generic;
using UnityEditor;

//#nullable enable

namespace MomomaAssets.GraphView
{
    public delegate INodeProcessorEditor GenerateNodeProcessorEditor(INodeProcessor nodeProcessor, SerializedNodeProcessor serializedNodeProcessor);

    static class NodeProcessorEditorFactory
    {
        sealed class EntryEditorFactory : IEntryDelegate<GenerateNodeProcessorEditor>
        {
            public readonly Dictionary<Type, GenerateNodeProcessorEditor> factories;

            public EntryEditorFactory(int capacity = 0)
            {
                factories = new Dictionary<Type, GenerateNodeProcessorEditor>(capacity);
            }

            public void Add(Type type, GenerateNodeProcessorEditor function)
            {
                factories.Add(type, function);
            }
        }

        static readonly IReadOnlyDictionary<Type, GenerateNodeProcessorEditor> s_Factories;

        static NodeProcessorEditorFactory()
        {
            var methods = TypeCache.GetMethodsWithAttribute<NodeProcessorEditorFactoryAttribute>();
            var factories = new EntryEditorFactory(methods.Count);
            var parameters = new object[] { factories };
            foreach (var methodInfo in methods)
                methodInfo.Invoke(null, parameters);
            s_Factories = factories.factories;
        }

        public static INodeProcessorEditor GetEditor(INodeProcessor processor, SerializedNodeProcessor serializedNodeProcessor)
        {
            if (s_Factories.TryGetValue(processor.GetType(), out var factory))
            {
                return factory(processor, serializedNodeProcessor);
            }
            return new DefaultNodeProcessorEditor(serializedNodeProcessor);
        }
    }
}
