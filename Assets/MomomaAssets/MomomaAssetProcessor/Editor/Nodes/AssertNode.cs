using System;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Log/Assert")]
    sealed class AssertNode : INodeProcessor
    {
        static AssertNode()
        {
            INodeDataUtility.AddConstructor(() => new AssertNode());
        }

        AssertNode() { }

        [SerializeField]
        string m_Message = "{0} is an invalid asset";

        public INodeProcessorEditor ProcessorEditor => new DefaultNodeProcessorEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(UnityObject), isMulti: true));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            foreach (var assets in assetGroup)
            {
                Debug.LogAssertionFormat(assets.MainAsset, m_Message, assets.MainAsset.name);
            }
        }
    }
}
