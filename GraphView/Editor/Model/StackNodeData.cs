using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

#nullable enable

namespace MomomaAssets.GraphView
{
    [Serializable]
    sealed class StackNodeData : IStackNodeData
    {
        [SerializeField]
        List<string> m_IncludingGuids = new List<string>();

        public string GraphElementName => "Stack Node";

        public int Priority => 1;

        public StackNodeData() { }

        public void InsertElements(int index, IEnumerable<string> guids) => m_IncludingGuids.InsertRange(index, guids);

        public void RemoveElements(IEnumerable<string> guids)
        {
            foreach (var guid in guids)
                m_IncludingGuids.Remove(guid);
        }

        public GraphElement Deserialize() => new BindableStackNode(this);

        public void SetPosition(GraphElement graphElement, Rect position) => graphElement.SetPosition(position);

        public void DeserializeOverwrite(GraphElement graphElement, UnityEditor.Experimental.GraphView.GraphView graphView)
        {
            if (!(graphElement is StackNode stackNode))
                throw new InvalidOperationException();
            var nodes = graphView.nodes.ToList().ToDictionary(node => node.viewDataKey);
            foreach (var e in stackNode.contentContainer.Query<Node>().ToList())
            {
                stackNode.RemoveElement(e);
                graphView.AddElement(e);
            }
            foreach (var guid in m_IncludingGuids)
                stackNode.AddElement(nodes[guid]);
        }

        public void ReplaceGuid(Dictionary<string, string> guids)
        {
            var newGuids = new List<string>();
            foreach (var guid in m_IncludingGuids)
            {
                if (!guids.TryGetValue(guid, out var newGuid))
                {
                    newGuid = Guid.NewGuid().ToString();
                    guids.Add(guid, newGuid);
                }
                newGuids.Add(newGuid);
            }
            m_IncludingGuids = newGuids;
        }

        public T DoFunction<T>(IFunctionContainer<IGraphElementData, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
