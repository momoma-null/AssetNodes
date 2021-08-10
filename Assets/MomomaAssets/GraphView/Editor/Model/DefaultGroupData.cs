using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;

    [Serializable]
    public class DefaultGroupData : IGroupData
    {
        [SerializeField]
        string m_Name = "New Group";
        [SerializeField]
        string[] m_IncludingGuids;

        DefaultGraphElementEditor? m_Editor;

        public int Priority => 1;
        public IGraphElementEditor GraphElementEditor => m_Editor ?? (m_Editor = new DefaultGraphElementEditor());
        public string Name => m_Name;
        public IEnumerable<string> IncludingGuids => m_IncludingGuids;

        public DefaultGroupData(string[] guids)
        {
            m_IncludingGuids = guids;
        }

        public GraphElement Deserialize() => new DefaultGroup(this);

        public void DeserializeOverwrite(GraphElement graphElement, GraphView graphView)
        {
            if (!(graphElement is Group group))
                throw new InvalidOperationException();
            group.title = m_Name;
            var elements = new HashSet<string>(m_IncludingGuids);
            group.RemoveElements(group.containedElements.Where(e => !elements.Contains(e.viewDataKey)));
            elements.ExceptWith(group.containedElements.Select(e => e.viewDataKey));
            var allElements = graphView.graphElements.ToList();
            group.AddElements(allElements.Where(e => elements.Contains(e.viewDataKey)));
        }

        public void ReplaceGuid(Dictionary<string, string> guids)
        {
            for (var i = 0; i < m_IncludingGuids.Length; ++i)
            {
                var guid = m_IncludingGuids[i];
                if (!guids.TryGetValue(guid, out var newGuid))
                {
                    newGuid = PortData.GetNewId();
                    guids.Add(guid, newGuid);
                }
                m_IncludingGuids[i] = newGuid;
            }
        }
    }
}
