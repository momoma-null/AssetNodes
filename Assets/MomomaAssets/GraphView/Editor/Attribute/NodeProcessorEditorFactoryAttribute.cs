using System;

//#nullable enable

namespace MomomaAssets.GraphView
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class NodeProcessorEditorFactoryAttribute : Attribute
    {
        public NodeProcessorEditorFactoryAttribute() { }
    }
}
