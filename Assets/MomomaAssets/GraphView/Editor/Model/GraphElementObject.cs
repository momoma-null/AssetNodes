﻿using System;
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
        [SerializeReference]
        IGraphElementData? m_GraphElementData;

        SerializedObject? m_SerializedObject;

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
                if (m_Position != value && m_SerializedObject != null)
                {
                    m_SerializedObject.Update();
                    using (var sp = m_SerializedObject.FindProperty(nameof(m_Position)))
                        sp.rectValue = value;
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
                    m_GraphElementData?.GraphElementEditor.OnDestroy();
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
            hideFlags = HideFlags.HideInHierarchy;
            if (!EditorUtility.IsPersistent(this))
                hideFlags |= HideFlags.DontSaveInEditor;
        }

        void OnEnable()
        {
            m_SerializedObject = new SerializedObject(this);
        }

        void OnDisable()
        {
            m_SerializedObject?.Dispose();
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
        [SerializeReference]
        IGraphElementData? m_GraphElementData;

        public string Guid { get => m_Guid; set => m_Guid = value; }
        public Rect Position { get => m_Position; set => m_Position = value; }
        public IGraphElementData? GraphElementData { get => m_GraphElementData; set => m_GraphElementData = value; }

        public SerializedGraphElement() { }
    }
}
