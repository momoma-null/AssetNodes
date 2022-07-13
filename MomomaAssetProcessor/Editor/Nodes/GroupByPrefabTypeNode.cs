using System;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Group/Group by PrefabType")]
    sealed class GroupByPrefabTypeNode : INodeProcessor
    {
        GroupByPrefabTypeNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(PrefabAssetType.Regular));
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(PrefabAssetType.Model));
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(PrefabAssetType.Variant));
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

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
