using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [Serializable]
    [CreateElement("Modify/Convert Material")]
    sealed class ConvertMaterialNode : INodeProcessor
    {
        static ConvertMaterialNode()
        {
            INodeDataUtility.AddConstructor(() => new ConvertMaterialNode());
        }

        ConvertMaterialNode() { }

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();
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
            var assetGroup = container.Get(m_InputPort.Id, () => new AssetGroup());
            if (m_DestinationShader != null)
            {
                foreach (var assets in assetGroup)
                {
                    foreach (var material in assets.GetAssetsFromType<Material>())
                    {
                        if (m_SourceShader != null && material.shader != m_SourceShader)
                            continue;
                        material.shader = m_DestinationShader;
                        if (EditorUtility.IsDirty(material))
                            EditorUtility.SetDirty(material);
                    }
                }
            }
            container.Set(m_OutputPort.Id, assetGroup);
        }
    }
}
