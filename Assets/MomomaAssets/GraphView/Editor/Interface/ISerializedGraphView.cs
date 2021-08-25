using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface ISerializedGraphView
    {
        IReadOnlyList<ISerializedGraphElement> SerializedGraphElements { get; }
    }

    [Serializable]
    public sealed class SerializedGraphView : ISerializedGraphView
    {
        public SerializedGraphView(List<SerializedGraphElement> serializedGraphElements) => m_SerializedGraphElements = serializedGraphElements;

        [SerializeField]
        List<SerializedGraphElement> m_SerializedGraphElements;

        public IReadOnlyList<ISerializedGraphElement> SerializedGraphElements => m_SerializedGraphElements;
    }
}
