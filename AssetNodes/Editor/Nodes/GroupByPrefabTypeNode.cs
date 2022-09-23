using System;
using UnityEditor;
using UnityEngine;

//#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Group/Group by PrefabType")]
    sealed class GroupByPrefabTypeNode : INodeProcessor
    {
        GroupByPrefabTypeNode() { }

        public Color HeaderColor => ColorDefinition.FilterNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(PrefabAssetType.Regular));
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(PrefabAssetType.Model));
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(PrefabAssetType.Variant));
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
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
            container.SetOutput(0, regulars);
            container.SetOutput(1, models);
            container.SetOutput(2, variants);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
