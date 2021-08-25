using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#nullable enable

namespace MomomaAssets.GraphView
{
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

        public event Action? onValueChanged;
        public Type? GraphViewType { get; private set; }
        public IReadOnlyList<ISerializedGraphElement> SerializedGraphElements => m_SerializedGraphElements;

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
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
