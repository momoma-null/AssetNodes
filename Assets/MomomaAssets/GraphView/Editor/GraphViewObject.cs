using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class GraphViewObject : ScriptableObject, ISerializedGraphView, ISerializationCallbackReceiver
    {
        [SerializeField]
        string m_GraphViewTypeName = "";
        [SerializeField]
        GraphElementObject[] m_SerializedGraphElements = new GraphElementObject[0];

        public Type? GraphViewType { get; private set; }
        public IEnumerable<ISerializedGraphElement> SerializedGraphElements => m_SerializedGraphElements;
        public IReadOnlyDictionary<string, int> GuidToIndices { get; private set; } = new Dictionary<string, int>();
        public IReadOnlyDictionary<string, ISerializedGraphElement> GuidToSerializedGraphElements { get; private set; } = new Dictionary<string, ISerializedGraphElement>();
        public event Action? onValueChanged;

        void OnValidate()
        {
            onValueChanged?.Invoke();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            GraphViewType = Type.GetType(m_GraphViewTypeName);
            var dict = new Dictionary<string, int>();
            var index = 0;
            foreach (var element in m_SerializedGraphElements)
            {
                if (element != null)
                    dict[element.Guid] = index;
                ++index;
            }
            GuidToIndices = dict;
            GuidToSerializedGraphElements = m_SerializedGraphElements.Where(i => i != null).ToDictionary(i => i.Guid, i => i as ISerializedGraphElement);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
