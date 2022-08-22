using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Clean up/Playable Asset")]
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
            foreach (var asset in assetGroup)
            {
                if (asset.MainAssetType == typeof(PlayableAsset))
                {
                    var remaindAssets = new HashSet<UnityObject>(asset.AllAssets);
                    remaindAssets.ExceptWith(EditorUtility.CollectDependencies(new[] { asset.MainAsset }));
                    foreach (var i in remaindAssets)
                    {
                        if (i != null)
                            UnityEngine.Object.DestroyImmediate(i, true);
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
