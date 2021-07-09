using System;

#nullable enable

namespace MomomaAssets.GraphView
{
    [Serializable]
    public sealed class SerializableWrapper<T>
    {
        public T value;
        public SerializableWrapper(T initial) => value = initial;
        public static implicit operator T(SerializableWrapper<T> wrapper) => wrapper.value;
    }

    public static class SerializableWrapper
    {
        public static SerializableWrapper<T> Create<T>(T initial) => new SerializableWrapper<T>(initial);
    }
}
