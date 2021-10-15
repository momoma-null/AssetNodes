using System;
using UnityEngine;
using UnityEditor;

//#nullable enable

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
        Shader m_SourceShader;
        [SerializeField]
        Shader m_DestinationShader;

        public INodeProcessorEditor ProcessorEditor { get; } = new DefaultNodeProcessorEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<Material>(isMulti: true);
            portDataContainer.AddOutputPort<Material>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
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
