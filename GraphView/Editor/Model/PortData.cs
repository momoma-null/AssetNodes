using System;
using UnityEngine;

//#nullable enable

namespace MomomaAssets.GraphView
{
    [Serializable]
    public sealed class PortData
    {
        public static string GetNewId() => Guid.NewGuid().ToString();

        [SerializeField]
        string m_PortType;
        [SerializeField]
        string m_PortName;
        [SerializeField]
        string m_Id;
        [SerializeField]
        bool m_IsMulti;
        [SerializeField]
        Color m_Color;

        public Type PortType => Type.GetType(m_PortType);
        public string PortName => m_PortName;
        public string Id { get => m_Id; set => m_Id = value; }
        public bool IsMulti => m_IsMulti;
        public Color Color => m_Color;

        public PortData(Type type, string name, bool isMulti, Color color)
        {
            m_PortType = type.AssemblyQualifiedName;
            m_PortName = name;
            m_IsMulti = isMulti;
            m_Color = color;
            m_Id = GetNewId();
        }
    }
}
