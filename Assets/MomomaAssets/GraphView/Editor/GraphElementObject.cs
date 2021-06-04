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
        string m_TypeName = "";
        [SerializeField]
        Rect m_Position = Rect.zero;
        [SerializeField]
        List<string> m_ReferenceGuids = new List<string>();
        [SerializeReference]
        IGraphElementData? m_GraphElementData;

        SerializedObject? m_SerializedObject;
        SerializedProperty? m_GuidProperty;
        SerializedProperty? m_TypeNameProperty;
        SerializedProperty? m_PositionProperty;
        SerializedProperty? m_ReferenceGuidsProperty;
        SerializedProperty? m_GraphElementDataProperty;

        public event Action<string>? onValueChanged;

        public string Guid
        {
            get => m_Guid;
            set
            {
                if (m_Guid != value)
                {
                    m_SerializedObject?.Update();
                    m_GuidProperty!.stringValue = value;
                    m_SerializedObject?.ApplyModifiedProperties();
                }
            }
        }

        public string TypeName
        {
            get => m_TypeName;
            set
            {
                if (m_TypeName != value)
                {
                    m_SerializedObject?.Update();
                    m_TypeNameProperty!.stringValue = value;
                    m_SerializedObject?.ApplyModifiedPropertiesWithoutUndo();
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
                if (m_GraphElementData != value)
                {
                    m_SerializedObject?.Update();
                    m_GraphElementDataProperty!.managedReferenceValue = value;
                    m_SerializedObject?.ApplyModifiedProperties();
                }
            }
        }

        void OnValidate()
        {
            onValueChanged?.Invoke(Guid);
        }

        void Awake()
        {
            hideFlags = HideFlags.DontSave;
        }

        void OnEnable()
        {
            m_SerializedObject = new SerializedObject(this);
            m_GuidProperty = m_SerializedObject.FindProperty(nameof(m_Guid));
            m_TypeNameProperty = m_SerializedObject.FindProperty(nameof(m_TypeName));
            m_PositionProperty = m_SerializedObject.FindProperty(nameof(m_Position));
            m_ReferenceGuidsProperty = m_SerializedObject.FindProperty(nameof(m_ReferenceGuids));
            m_GraphElementDataProperty = m_SerializedObject.FindProperty(nameof(m_GraphElementData));
        }

        void OnDisable()
        {
            m_GuidProperty?.Dispose();
            m_TypeNameProperty?.Dispose();
            m_PositionProperty?.Dispose();
            m_ReferenceGuidsProperty?.Dispose();
            m_GraphElementDataProperty?.Dispose();
            m_SerializedObject?.Dispose();
            m_GuidProperty = null;
            m_TypeNameProperty = null;
            m_PositionProperty = null;
            m_ReferenceGuidsProperty = null;
            m_GraphElementDataProperty = null;
            m_SerializedObject = null;
        }
    }

    [Serializable]
    public sealed class SerializedGraphElement : ISerializedGraphElement
    {
        [SerializeField]
        string m_Guid = "";
        [SerializeField]
        string m_TypeName = "";
        [SerializeField]
        Rect m_Position;
        [SerializeField]
        List<string> m_ReferenceGuids = new List<string>();
        [SerializeReference]
        IGraphElementData? m_GraphElementData;

        public string Guid { get => m_Guid; set => m_Guid = value; }
        public string TypeName { get => m_TypeName; set => m_TypeName = value; }
        public Rect Position { get => m_Position; set => m_Position = value; }
        public IReadOnlyList<string> ReferenceGuids { get => m_ReferenceGuids; set { m_ReferenceGuids.Clear(); m_ReferenceGuids.AddRange(value); } }
        public IGraphElementData? GraphElementData { get => m_GraphElementData; set => m_GraphElementData = value; }

        public SerializedGraphElement() { }
    }
}
