using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Clean up/Playable Asset")]
    sealed class CleanUpPlayableAssetNode : INodeProcessor
    {
        static CleanUpPlayableAssetNode()
        {
            INodeDataUtility.AddConstructor(() => new CleanUpPlayableAssetNode());
        }

        CleanUpPlayableAssetNode() { }

        public INodeProcessorEditor ProcessorEditor => new DefaultNodeProcessorEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(UnityObject)));
            portDataContainer.OutputPorts.Add(new PortData(typeof(UnityObject)));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], this.NewAssetGroup);
            foreach (var asset in assetGroup)
            {
                if (asset.MainAsset is PlayableAsset)
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
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
