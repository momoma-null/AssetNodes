using System;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Group/Filter by Regex")]
    sealed class FilterByRegexNode : INodeProcessor
    {
        [SerializeField]
        string m_Pattern = string.Empty;

        FilterByRegexNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assets = container.GetInput(0, AssetGroupPortDefinition.Default);
            var input = container.GetInput(1, PathDataPortDefinition.Default);
            var regex = new Regex(m_Pattern);
            assets.RemoveWhere(asset => !regex.IsMatch(input.GetPath(asset)));
            container.SetOutput(0, assets);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
