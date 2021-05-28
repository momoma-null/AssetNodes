using System;
using UnityObject = UnityEngine.Object;


#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class UnityObjectWrapper
    {
        public static UnityObjectWrapper Create<T>(T? target = null) where T : UnityObject
        {
            return new UnityObjectWrapper(typeof(T), target);
        }

        public Type ObjectType {get;}
        public UnityObject? Target { get; }

        UnityObjectWrapper(Type type, UnityObject? target)
        {
            ObjectType = type;
            Target = target;
        }
    }
}
