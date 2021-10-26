using System;
using System.Collections.Generic;
using UnityEditor;

//#nullable enable

namespace MomomaAssets.GraphView
{
    interface INodeData : IGraphElementData, IPortDataContainer
    {
        bool Expanded { get; }
        INodeProcessor Processor { get; }
    }

    public static class INodeDataUtility
    {
        static Dictionary<Type, Func<INodeProcessor>> s_Constructors = new Dictionary<Type, Func<INodeProcessor>>();

        public static IReadOnlyCollection<Func<INodeProcessor>> Constructors => s_Constructors.Values;

        public static void AddConstructor<TNode>(Func<TNode> ctor) where TNode : INodeProcessor
        {
            s_Constructors[typeof(TNode)] = () => ctor();
        }
    }
}
