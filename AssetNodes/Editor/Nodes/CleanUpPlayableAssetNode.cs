using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Clean up/Playable Asset")]
    sealed class CleanUpPlayableAssetNode : INodeProcessor
    {
        CleanUpPlayableAssetNode() { }

        public Color HeaderColor => ColorDefinition.CleanupNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            if (assetGroup.Count == 0)
                return;
            using (new AssetModificationScope())
            {
                foreach (var asset in assetGroup)
                {
                    if (typeof(PlayableAsset).IsAssignableFrom(asset.MainAssetType))
                    {
                        var remainedAssets = new HashSet<UnityObject>(asset.AllAssets);
                        remainedAssets.ExceptWith(EditorUtility.CollectDependencies(new[] { asset.MainAsset }));
                        foreach (var i in remainedAssets)
                        {
                            if (i != null)
                                UnityEngine.Object.DestroyImmediate(i, true);
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
