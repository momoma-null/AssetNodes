using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MomomaAssets.Extensions;

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

        public string Guid
        {
            get => m_Guid;
            set
            {
                m_SerializedObject?.Update();
                m_GuidProperty!.stringValue = value;
                m_SerializedObject?.ApplyModifiedProperties();
            }
        }

        public string TypeName
        {
            get => m_TypeName;
            set
            {
                m_SerializedObject?.Update();
                m_TypeNameProperty!.stringValue = value;
                m_SerializedObject?.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public Rect Position
        {
            get => m_Position;
            set
            {
                m_SerializedObject?.Update();
                m_PositionProperty!.rectValue = value;
                m_SerializedObject?.ApplyModifiedProperties();
            }
        }

        public IList<string> ReferenceGuids { get; private set; } = Array.Empty<string>();

        public IGraphElementData? GraphElementData
        {
            get => m_GraphElementData;
            set
            {
                m_SerializedObject?.Update();
                m_GraphElementDataProperty!.managedReferenceValue = value;
                m_SerializedObject?.ApplyModifiedProperties();
            }
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
            ReferenceGuids = new SerializedPropertyList<string>(m_ReferenceGuidsProperty, sp => sp.stringValue, (sp, val) => sp.stringValue = val);
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
        public IList<string> ReferenceGuids => m_ReferenceGuids;
        public IGraphElementData? GraphElementData { get => m_GraphElementData; set => m_GraphElementData = value; }

        public SerializedGraphElement() { }
    }
}
