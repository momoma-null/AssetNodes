using System;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Modify/Convert Material")]
    sealed class ConvertMaterialNode : INodeProcessor
    {
        ConvertMaterialNode() { }

        [SerializeField]
        Shader? m_SourceShader;
        [SerializeField]
        Shader? m_DestinationShader;

        public Color HeaderColor => ColorDefinition.ModifyNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            container.SetOutput(0, assetGroup);
            if (assetGroup.Count == 0)
                return;
            if (m_DestinationShader != null)
            {
                using (new AssetModificationScope())
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
            }
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
