using System;
using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface INodeData : IGraphElementData
    {
        string Title { get; }
        string MenuPath { get; }
        IEnumerable<PortData> InputPorts { get; }
        IEnumerable<PortData> OutputPorts { get; }
    }

    public static class INodeDataUtility
    {
        static Dictionary<Type, Func<INodeData>> s_Constructors = new Dictionary<Type, Func<INodeData>>();

        public static IReadOnlyCollection<Func<INodeData>> Constructors => s_Constructors.Values;

        public static void AddConstructor<TNode>(Func<TNode> ctor) where TNode : INodeData
        {
            s_Constructors[typeof(TNode)] = () => ctor() as INodeData;
        }
    }
}
