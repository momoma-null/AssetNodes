using System;
using UnityEngine;

#nullable enable

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

        public Type PortType => Type.GetType(m_PortType);
        public string PortTypeName => m_PortName;
        public string PortName => m_PortName;
        public string Id { get => m_Id; set => m_Id = value; }

        public PortData(Type type, string name = "", string? id = null)
        {
            m_PortType = type.AssemblyQualifiedName;
            m_PortName = name;
            m_Id = id ?? GetNewId();
        }
    }
}
