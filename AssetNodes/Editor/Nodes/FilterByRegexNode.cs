using System;
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
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, "Matched");
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, "Unmatched");
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            var input = container.GetInput(1, PathDataPortDefinition.Default);
            var regex = new Regex(m_Pattern);
            var matched = new AssetGroup();
            var unmatched = new AssetGroup();
            foreach (var asset in assetGroup)
            {
                if (regex.IsMatch(input.GetPath(asset)))
                    matched.Add(asset);
                else
                    unmatched.Add(asset);
            }
            container.SetOutput(0, matched);
            container.SetOutput(1, unmatched);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
