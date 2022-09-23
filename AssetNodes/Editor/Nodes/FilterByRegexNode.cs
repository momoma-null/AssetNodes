using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

//#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Group/Filter by Regex")]
    sealed class FilterByRegexNode : INodeProcessor
    {
        [SerializeField]
        string m_Pattern = string.Empty;

        public Color HeaderColor => ColorDefinition.FilterNode;

        FilterByRegexNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            var input = container.GetInput(1, PathDataPortDefinition.Default);
            var regex = new Regex(m_Pattern);
            var filtered = new AssetGroup(assetGroup.Where(asset => regex.IsMatch(input.GetPath(asset))));
            container.SetOutput(0, filtered);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
