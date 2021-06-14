using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface ISerializedGraphView
    {
        IEnumerable<ISerializedGraphElement> SerializedGraphElements { get; }
    }

    [Serializable]
    public sealed class SerializedGraphView : ISerializedGraphView
    {
        public SerializedGraphView(List<SerializedGraphElement> serializedGraphElements) => m_SerializedGraphElements = serializedGraphElements;

        [SerializeField]
        List<SerializedGraphElement> m_SerializedGraphElements;

        public IEnumerable<ISerializedGraphElement> SerializedGraphElements => m_SerializedGraphElements;
    }
}
