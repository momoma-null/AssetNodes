using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#nullable enable

namespace MomomaAssets.GraphView
{
    [DefaultExecutionOrder(-10)]
    public sealed partial class GraphViewObject : ScriptableObject, ISerializedGraphView, ISerializationCallbackReceiver
    {
        static readonly Dictionary<Type, HashSet<GraphViewObject>> s_AllGraphViewObjects = new Dictionary<Type, HashSet<GraphViewObject>>();

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

        public Type? GraphViewType { get; private set; }
        IReadOnlyList<ISerializedGraphElement> ISerializedGraphView.SerializedGraphElements => m_SerializedGraphElements;
        IReadOnlyDictionary<string, ISerializedGraphElement> ISerializedGraphView.GuidtoSerializedGraphElements => m_GuidtoSerializedGraphElements;

        public static IReadOnlyCollection<GraphViewObject> GetGraphViewObjects<T>() where T : EditorWindow
        {
            if (s_AllGraphViewObjects.TryGetValue(typeof(T), out var objects))
                return objects;
            return Array.Empty<GraphViewObject>();
        }

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(GraphViewObject)}"))
            {
                AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        void OnEnable()
        {
            // to wait for the deserialization of GraphElementObject
            m_GuidtoSerializedGraphElements.Clear();
            foreach (var i in m_SerializedGraphElements)
                m_GuidtoSerializedGraphElements.Add(i.Guid, i);
        }

        void OnDestroy()
        {
            UnregisterSelf();
        }

        void RegisterSelf()
        {
            if (GraphViewType != null)
            {
                if (!s_AllGraphViewObjects.TryGetValue(GraphViewType, out var objects))
                {
                    objects = new HashSet<GraphViewObject>();
                    s_AllGraphViewObjects.Add(GraphViewType, objects);
                }
                objects.Add(this);
            }
        }

        void UnregisterSelf()
        {
            if (GraphViewType != null)
            {
                if (s_AllGraphViewObjects.TryGetValue(GraphViewType, out var objects))
                {
                    objects.Remove(this);
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            var type = Type.GetType(m_GraphViewTypeName);
            if (GraphViewType != type)
            {
                UnregisterSelf();
                GraphViewType = type;
                RegisterSelf();
            }
            m_GuidtoSerializedGraphElements.Clear();
            foreach (var i in m_SerializedGraphElements)
                m_GuidtoSerializedGraphElements.Add(i.Guid, i);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
