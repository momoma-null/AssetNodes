using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class GraphElementObject : ScriptableObject, ISerializedGraphElement
    {
        GraphElementObject() { }

        [SerializeField]
        string m_Guid = "";
        [SerializeField]
        Rect m_Position = Rect.zero;
        [SerializeField]
        List<string> m_ReferenceGuids = new List<string>();
        [SerializeReference]
        IGraphElementData? m_GraphElementData;

        SerializedObject? m_SerializedObject;
        SerializedProperty? m_PositionProperty;
        SerializedProperty? m_ReferenceGuidsProperty;

        public event Action<string>? onValueChanged;

        public string Guid
        {
            get => m_Guid;
            set
            {
                if (m_Guid != value && m_SerializedObject != null)
                {
                    m_SerializedObject.Update();
                    using (var sp = m_SerializedObject.FindProperty(nameof(m_Guid)))
                        sp.stringValue = value;
                    using (var sp = m_SerializedObject.FindProperty("m_Name"))
                        sp.stringValue = value;
                    m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        public Rect Position
        {
            get => m_Position;
            set
            {
                if (m_Position != value)
                {
                    m_SerializedObject?.Update();
                    m_PositionProperty!.rectValue = value;
                    m_SerializedObject?.ApplyModifiedProperties();
                }
            }
        }

        public IReadOnlyList<string> ReferenceGuids
        {
            get => m_ReferenceGuids;
            set
            {
                if (m_ReferenceGuids.Count != value.Count || !m_ReferenceGuids.SequenceEqual(value))
                {
                    m_SerializedObject?.Update();
                    m_ReferenceGuidsProperty!.ClearArray();
                    m_ReferenceGuidsProperty.arraySize = value.Count;
                    for (var i = 0; i < value.Count; ++i)
                        m_ReferenceGuidsProperty.GetArrayElementAtIndex(i).stringValue = value[i];
                    m_SerializedObject?.ApplyModifiedProperties();
                }
            }
        }

        public IGraphElementData? GraphElementData
        {
            get => m_GraphElementData;
            set
            {
                if (m_GraphElementData != value && m_SerializedObject != null)
                {
                    m_SerializedObject.Update();
                    using (var sp = m_SerializedObject.FindProperty(nameof(m_GraphElementData)))
                        sp.managedReferenceValue = value;
                    m_SerializedObject.ApplyModifiedProperties();
                }
            }
        }

        void OnValidate()
        {
            onValueChanged?.Invoke(Guid);
        }

        void Awake()
        {
            //hideFlags = HideFlags.HideInHierarchy;
            if (!EditorUtility.IsPersistent(this))
                hideFlags |= HideFlags.DontSaveInEditor;
        }

        void OnEnable()
        {
            m_SerializedObject = new SerializedObject(this);
            m_PositionProperty = m_SerializedObject.FindProperty(nameof(m_Position));
            m_ReferenceGuidsProperty = m_SerializedObject.FindProperty(nameof(m_ReferenceGuids));
        }

        void OnDisable()
        {
            m_PositionProperty?.Dispose();
            m_ReferenceGuidsProperty?.Dispose();
            m_SerializedObject?.Dispose();
            m_PositionProperty = null;
            m_ReferenceGuidsProperty = null;
            m_SerializedObject = null;
        }
    }

    [Serializable]
    public sealed class SerializedGraphElement : ISerializedGraphElement
    {
        [SerializeField]
        string m_Guid = "";
        [SerializeField]
        Rect m_Position;
        [SerializeField]
        List<string> m_ReferenceGuids = new List<string>();
        [SerializeReference]
        IGraphElementData? m_GraphElementData;

        public string Guid { get => m_Guid; set => m_Guid = value; }
        public Rect Position { get => m_Position; set => m_Position = value; }
        public IReadOnlyList<string> ReferenceGuids { get => m_ReferenceGuids; set { m_ReferenceGuids.Clear(); m_ReferenceGuids.AddRange(value); } }
        public IGraphElementData? GraphElementData { get => m_GraphElementData; set => m_GraphElementData = value; }

        public SerializedGraphElement() { }
    }
}
