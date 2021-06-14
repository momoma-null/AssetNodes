using System;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IEdgeData : IGraphElementData
    {
        string InputPortGuid { get; set; }
        string OutputPortGuid { get; set; }
    }

    [Serializable]
    public class DefaultEdgeData : IEdgeData
    {
        public DefaultEdgeData(string input, string output)
        {
            m_InputPortGuid = input;
            m_OutputPortGuid = output;
        }

        [SerializeField]
        string m_InputPortGuid;
        [SerializeField]
        string m_OutputPortGuid;

        public string InputPortGuid { get => m_InputPortGuid; set => m_InputPortGuid = value; }
        public string OutputPortGuid { get => m_OutputPortGuid; set => m_OutputPortGuid = value; }
    }
}
