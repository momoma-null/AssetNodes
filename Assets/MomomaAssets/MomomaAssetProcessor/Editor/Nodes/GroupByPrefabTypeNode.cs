using System;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Group/Group by PrefabType")]
    sealed class GroupByPrefabTypeNode : INodeProcessor
    {
        static GroupByPrefabTypeNode()
        {
            INodeDataUtility.AddConstructor(() => new GroupByPrefabTypeNode());
        }

        GroupByPrefabTypeNode() { }

        public INodeProcessorEditor ProcessorEditor { get; } = new DefaultNodeProcessorEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(GameObject), isMulti: true));
            portDataContainer.OutputPorts.Add(new PortData(typeof(GameObject), nameof(PrefabAssetType.Regular), isMulti: true));
            portDataContainer.OutputPorts.Add(new PortData(typeof(GameObject), nameof(PrefabAssetType.Model), isMulti: true));
            portDataContainer.OutputPorts.Add(new PortData(typeof(GameObject), nameof(PrefabAssetType.Variant), isMulti: true));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            var regulars = new AssetGroup();
            var models = new AssetGroup();
            var variants = new AssetGroup();
            foreach (var assets in assetGroup)
            {
                switch (PrefabUtility.GetPrefabAssetType(assets.MainAsset))
                {
                    case PrefabAssetType.Regular: regulars.Add(assets); break;
                    case PrefabAssetType.Model: models.Add(assets); break;
                    case PrefabAssetType.Variant: variants.Add(assets); break;
                }
            }
            container.Set(portDataContainer.OutputPorts[0], regulars);
            container.Set(portDataContainer.OutputPorts[1], models);
            container.Set(portDataContainer.OutputPorts[2], variants);
        }
    }
}
