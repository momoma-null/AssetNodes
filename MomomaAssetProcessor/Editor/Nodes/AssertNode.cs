using System;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Log/Assert")]
    sealed class AssertNode : INodeProcessor
    {
        AssertNode() { }

        [SerializeField]
        string m_Message = "{0} is an invalid asset";

        public Color HeaderColor => ColorDefinition.ValidateNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            foreach (var assets in assetGroup)
            {
                Debug.LogAssertionFormat(assets.MainAsset, m_Message, assets.MainAsset.name);
            }
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
