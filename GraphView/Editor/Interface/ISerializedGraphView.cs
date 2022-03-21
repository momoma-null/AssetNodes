using System;
using System.Collections.Generic;
using UnityEngine;

//#nullable enable

namespace MomomaAssets.GraphView
{
    interface ISerializedGraphView
    {
        IReadOnlyList<ISerializedGraphElement> SerializedGraphElements { get; }
        IReadOnlyDictionary<string, ISerializedGraphElement> GuidtoSerializedGraphElements { get; }
    }

    [Serializable]
    sealed class SerializedGraphView : ISerializedGraphView, ISerializationCallbackReceiver
    {
        public SerializedGraphView(List<SerializedGraphElement> serializedGraphElements) => m_SerializedGraphElements = serializedGraphElements;

        [SerializeField]
        List<SerializedGraphElement> m_SerializedGraphElements;

        [NonSerialized]
        Dictionary<string, ISerializedGraphElement> m_GuidtoSerializedGraphElements = new Dictionary<string, ISerializedGraphElement>();

        public IReadOnlyList<ISerializedGraphElement> SerializedGraphElements => m_SerializedGraphElements;
        public IReadOnlyDictionary<string, ISerializedGraphElement> GuidtoSerializedGraphElements => m_GuidtoSerializedGraphElements;

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_GuidtoSerializedGraphElements.Clear();
            foreach (var i in m_SerializedGraphElements)
                m_GuidtoSerializedGraphElements.Add(i.Guid, i);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
