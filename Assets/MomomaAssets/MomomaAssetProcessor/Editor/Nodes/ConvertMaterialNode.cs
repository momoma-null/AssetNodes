using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Modify/Convert Material")]
    sealed class ConvertMaterialNode : INodeProcessor
    {
        static ConvertMaterialNode()
        {
            INodeDataUtility.AddConstructor(() => new ConvertMaterialNode());
        }

        ConvertMaterialNode() { }

        [SerializeField]
        Shader? m_SourceShader;
        [SerializeField]
        Shader? m_DestinationShader;

        public INodeProcessorEditor ProcessorEditor { get; } = new DefaultNodeProcessorEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(Material)));
            portDataContainer.OutputPorts.Add(new PortData(typeof(Material)));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], this.NewAssetGroup);
            if (m_DestinationShader != null)
            {
                foreach (var assets in assetGroup)
                {
                    foreach (var material in assets.GetAssetsFromType<Material>())
                    {
                        if (m_SourceShader != null && material.shader != m_SourceShader || material.shader == m_DestinationShader)
                            continue;
                        material.shader = m_DestinationShader;
                        if (EditorUtility.IsDirty(material))
                            EditorUtility.SetDirty(material);
                    }
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
