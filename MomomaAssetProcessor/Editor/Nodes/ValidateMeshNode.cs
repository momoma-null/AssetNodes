using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Validate/Mesh")]
    sealed class ValidateMeshNode : INodeProcessor
    {
        [SerializeField]
        int m_PolyCountLimit = int.MaxValue;
        [SerializeField]
        VertexAttribute[] m_RequiredAttributes = Array.Empty<VertexAttribute>();
        [SerializeField]
        VertexAttribute[] m_UnnecessaryAttributes = Array.Empty<VertexAttribute>();

        ValidateMeshNode() { }

        public Color HeaderColor => ColorDefinition.ValidateNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, "Found Assets");
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            var foundAssets = new AssetGroup();
            foreach (var assets in assetGroup)
            {
                if (FindMissingReference(assets))
                    foundAssets.Add(assets);
            }
            container.SetOutput(0, foundAssets);
        }

        bool FindMissingReference(AssetData assetData)
        {
            var found = false;
            var attributes = new List<VertexAttributeDescriptor>();
            foreach (var mesh in assetData.GetAssetsFromType<Mesh>())
            {
                var triangleCount = mesh.triangles.Length / 3;
                if (triangleCount > m_PolyCountLimit)
                {
                    found = true;
                    Debug.LogError($"Invalid Mesh : {mesh.name} ({assetData.AssetPath}) has greater than {m_PolyCountLimit} triangles, {triangleCount}", mesh);
                }
                mesh.GetVertexAttributes(attributes);
                foreach (var attr in m_RequiredAttributes)
                {
                    if (attributes.FindIndex(x => x.attribute == attr) < 0)
                    {
                        found = true;
                        Debug.LogError($"Invalid Mesh : {mesh.name} ({assetData.AssetPath}) has no {attr} attribute", mesh);
                    }
                }
                foreach (var attr in m_UnnecessaryAttributes)
                {
                    if (attributes.FindIndex(x => x.attribute == attr) > -1)
                    {
                        found = true;
                        Debug.LogError($"Invalid Mesh : {mesh.name} ({assetData.AssetPath}) has {attr} attribute", mesh);
                    }
                }
            }
            return found;
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
