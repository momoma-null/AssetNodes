using System;
using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface INode
    {
        string Title { get; }
        IEnumerable<PortData> InputPorts { get; }
        IEnumerable<PortData> OutputPorts { get; }
        IEnumerable<PropertyValue> GetProperties();
    }

    public static class NodeUtility
    {
        static Dictionary<Type, Func<INode>> s_Constructors = new Dictionary<Type, Func<INode>>();

        public static IReadOnlyCollection<Func<INode>> Constructors => s_Constructors.Values;

        public static void AddConstructor<TNode>(Func<TNode> ctor) where TNode : INode
        {
            s_Constructors[typeof(TNode)] = () => ctor() as INode;
        }
    }
}
