using System;

#nullable enable

namespace MomomaAssets.GraphView
{
    interface IEntryDelegate<T> where T : Delegate
    {
        void Add(Type type, T function);
    }
}
