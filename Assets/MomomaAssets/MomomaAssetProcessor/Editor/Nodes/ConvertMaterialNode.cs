using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [Serializable]
    sealed class ConvertMaterialNode : INodeData
    {
        static ConvertMaterialNode()
        {
            INodeDataUtility.AddConstructor(() => new ConvertMaterialNode());
        }

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();
        public string MenuPath => "Modify/Convert Material";
        public IEnumerable<PortData> InputPorts => new[] { m_InputPort };
        public IEnumerable<PortData> OutputPorts => new[] { m_OutputPort };

        [SerializeField]
        [HideInInspector]
        PortData m_InputPort = new PortData(typeof(Material));

        [SerializeField]
        [HideInInspector]
        PortData m_OutputPort = new PortData(typeof(Material));

        [SerializeField]
        Shader? m_SourceShader;

        [SerializeField]
        Shader? m_DestinationShader;

        public void Process(ProcessingDataContainer container)
        {
            var assets = container.Get(m_InputPort.Id, () => new AssetGroup());
            if (m_DestinationShader != null)
            {
                foreach (var asset in assets)
                {
                    if (asset is Material material)
                    {
                        if (m_SourceShader != null && material.shader != m_SourceShader)
                            continue;
                        material.shader = m_DestinationShader;
                        if (EditorUtility.IsDirty(material))
                            EditorUtility.SetDirty(material);
                    }
                }
            }
            container.Set(m_OutputPort.Id, assets);
        }
    }
}
