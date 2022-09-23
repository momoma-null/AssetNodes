using System;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Path/Match Name")]
    sealed class MatchNameNode : INodeProcessor
    {
        MatchNameNode() { }

        [SerializeField]
        string m_RegexPattern = string.Empty;
        [SerializeField]
        bool m_UseFirstGroup;

        public Color HeaderColor => ColorDefinition.PathNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var path = container.GetInput(0, PathDataPortDefinition.Default);
            var regex = new Regex(m_RegexPattern);
            container.SetOutput(0, new PathData(asset => regex.Match(path.GetPath(asset)).Groups[m_UseFirstGroup ? 1 : 0].Value));
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
