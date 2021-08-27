using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#nullable enable

namespace MomomaAssets.GraphView
{
    [DefaultExecutionOrder(-10)]
    public sealed class GraphViewObject : ScriptableObject, ISerializedGraphView, ISerializationCallbackReceiver
    {
        [OnOpenAsset]
        static bool OnOpenAsset(int instanceID, int line)
        {
            var target = EditorUtility.InstanceIDToObject(instanceID);
            if (!(target is GraphViewObject graphViewObject) || graphViewObject.GraphViewType == null)
                return false;
            EditorWindow.GetWindow(graphViewObject.GraphViewType, false, graphViewObject.GraphViewType.Name);
            return true;
        }

        [SerializeField]
        string m_GraphViewTypeName = "";
        [SerializeField]
        GraphElementObject[] m_SerializedGraphElements = new GraphElementObject[0];

        [NonSerialized]
        Dictionary<string, ISerializedGraphElement> m_GuidtoSerializedGraphElements = new Dictionary<string, ISerializedGraphElement>();

        public event Action? onValueChanged;
        public Type? GraphViewType { get; private set; }
        public IReadOnlyList<ISerializedGraphElement> SerializedGraphElements => m_SerializedGraphElements;
        public IReadOnlyDictionary<string, ISerializedGraphElement> GuidtoSerializedGraphElements => m_GuidtoSerializedGraphElements;

        void OnValidate()
        {
            onValueChanged?.Invoke();
        }

        void OnDestroy()
        {
            onValueChanged = null;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            GraphViewType = Type.GetType(m_GraphViewTypeName);
            m_GuidtoSerializedGraphElements.Clear();
            foreach (var i in m_SerializedGraphElements)
                m_GuidtoSerializedGraphElements.Add(i.Guid, i);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
