using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;

    [Serializable]
    class GroupData : IGroupData, ISerializationCallbackReceiver
    {
        [SerializeField]
        string m_Name = "New Group";
        [SerializeField]
        string[] m_IncludingGuids = Array.Empty<string>();

        [NonSerialized]
        HashSet<string> m_IncludingGuidSet;

        public string GraphElementName => $"{m_Name} Group";
        public int Priority => 1;
        public string Name => m_Name;
        public int ElementCount => m_IncludingGuidSet.Count;

        public GroupData(IEnumerable<string> guids)
        {
            m_IncludingGuidSet = new HashSet<string>(guids);
        }

        public void AddElements(IEnumerable<string> guids) => m_IncludingGuidSet.UnionWith(guids);

        public void RemoveElements(IEnumerable<string> guids) => m_IncludingGuidSet.ExceptWith(guids);

        public GraphElement Deserialize() => new BindableGroup(this);

        public void SetPosition(GraphElement graphElement, Rect position)
        {
            if (!(graphElement is BindableGroup group))
                throw new InvalidOperationException();
            group.SetPositionWhenDeserialization(position);
        }

        public void DeserializeOverwrite(GraphElement graphElement, GraphView graphView)
        {
            if (!(graphElement is Group group))
                throw new InvalidOperationException();
            if (m_IncludingGuidSet.Count == 0)
            {
                graphView.DeleteElements(new[] { group });
                return;
            }
            group.title = m_Name;
            var toRemoveElements = new HashSet<GraphElement>(group.containedElements);
            toRemoveElements.RemoveWhere(e => m_IncludingGuidSet.Contains(e.viewDataKey));
            group.RemoveElements(toRemoveElements);
            var toAddElements = new HashSet<GraphElement>(graphView.graphElements.ToList());
            toAddElements.ExceptWith(group.containedElements);
            group.AddElements(toAddElements.Where(e => m_IncludingGuidSet.Contains(e.viewDataKey)));
            group.UpdateGeometryFromContent();
        }

        public void ReplaceGuid(Dictionary<string, string> guids)
        {
            var newSet = new HashSet<string>();
            foreach (var guid in m_IncludingGuidSet)
            {
                if (!guids.TryGetValue(guid, out var newGuid))
                {
                    newGuid = Guid.NewGuid().ToString();
                    guids.Add(guid, newGuid);
                }
                newSet.Add(newGuid);
            }
            m_IncludingGuidSet = newSet;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_IncludingGuidSet == null)
                m_IncludingGuidSet = new HashSet<string>();
            else
                m_IncludingGuidSet.Clear();
            m_IncludingGuidSet.UnionWith(m_IncludingGuids);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (m_IncludingGuidSet == null)
                m_IncludingGuidSet = new HashSet<string>();
            m_IncludingGuids = new string[m_IncludingGuidSet.Count];
            m_IncludingGuidSet.CopyTo(m_IncludingGuids);
        }
    }
}
