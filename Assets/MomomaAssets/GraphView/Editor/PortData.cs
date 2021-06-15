using System;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    [Serializable]
    public sealed class PortData
    {
        [SerializeField]
        string m_PortType;
        [SerializeField]
        string m_PortName;
        [SerializeField]
        string m_Id;

        public Type PortType => Type.GetType(m_PortType);
        public string PortName => m_PortName;
        public string Id => m_Id;

        public PortData(Type type, string name = "", string? id = null)
        {
            m_PortType = type.AssemblyQualifiedName;
            m_PortName = name;
            m_Id = id ?? Guid.NewGuid().ToString();
        }
    }
}
