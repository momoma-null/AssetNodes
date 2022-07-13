using System;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Clean up/Playable Asset")]
    sealed class CleanUpPlayableAssetNode : INodeProcessor
    {
        CleanUpPlayableAssetNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroupPortDefinition.Default);
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
