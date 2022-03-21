using System;
using UnityEditor;

//#nullable enable

namespace MomomaAssets.GraphView
{
    public delegate INodeProcessorEditor CreateNodeProcessorEditor<T>(T nodeProcessor, SerializedNodeProcessor serializedNodeProcessor) where T : INodeProcessor;

    public static class NodeProcessorEditorFactory
    {
        static class CachedFactory<T> where T : INodeProcessor
        {
            public static CreateNodeProcessorEditor<T> factory;
        }

        sealed class FunctionProxy : IFunctionContainer<INodeProcessor, INodeProcessorEditor>
        {
            SerializedNodeProcessor _serializedNodeProcessor;

            public FunctionProxy(SerializedNodeProcessor serializedNodeProcessor)
            {
                _serializedNodeProcessor = serializedNodeProcessor;
            }

            public INodeProcessorEditor DoFunction<T>(T arg) where T : INodeProcessor
            {
                return CachedFactory<T>.factory?.Invoke(arg, _serializedNodeProcessor) ?? new DefaultNodeProcessorEditor(_serializedNodeProcessor);
            }
        }

        static NodeProcessorEditorFactory()
        {
            var methods = TypeCache.GetMethodsWithAttribute<NodeProcessorEditorFactoryAttribute>();
            foreach (var methodInfo in methods)
                methodInfo.Invoke(null, Array.Empty<object>());
        }

        public static void EntryEditorFactory<T>(CreateNodeProcessorEditor<T> createNodeProcessorEditor) where T : INodeProcessor
        {
            CachedFactory<T>.factory = createNodeProcessorEditor;
        }

        public static INodeProcessorEditor CreateEditor(INodeProcessor processor, SerializedNodeProcessor serializedNodeProcessor)
        {
            var functionProxy = new FunctionProxy(serializedNodeProcessor);
            return processor.DoFunction(functionProxy);
        }
    }
}
