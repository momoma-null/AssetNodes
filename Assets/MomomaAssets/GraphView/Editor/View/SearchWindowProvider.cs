using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;

    sealed class SearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        public event Action<GraphElement, Vector2>? addGraphElement;
        public Type graphViewType { get; set; } = typeof(GraphView);

        List<SearchTreeEntry>? m_SearchTree;

        void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            if (m_SearchTree != null)
                return m_SearchTree;
            m_SearchTree = new List<SearchTreeEntry>();
            m_SearchTree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));
            var nodes = new SortedList<string, Func<INodeProcessor>>(INodeDataUtility.Constructors.Count);
            foreach (var ctor in INodeDataUtility.Constructors)
            {
                var node = ctor();
                foreach (var attr in node.GetType().GetCustomAttributes(typeof(CreateElementAttribute), false))
                {
                    if (attr is CreateElementAttribute createElement)
                    {
                        nodes.Add(createElement.MenuPath, ctor);
                    }
                }
            }
            var groupPaths = new HashSet<string>();
            foreach (var node in nodes)
            {
                var names = new Queue<string>(node.Key.Split('/'));
                var level = 1;
                while (names.Count > 1)
                {
                    var entryName = names.Dequeue();
                    if (groupPaths.Add(entryName))
                    {
                        m_SearchTree.Add(new SearchTreeGroupEntry(new GUIContent(entryName), level));
                    }
                    ++level;
                }
                m_SearchTree.Add(new SearchTreeEntry(new GUIContent(names.Dequeue())) { level = level, userData = node.Value });
            }
            return m_SearchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var ctor = entry.userData as Func<INodeProcessor>;
            var processor = ctor?.Invoke();
            if (processor == null)
                return false;
            var graphElement = new NodeGUI(new DefaultNodeData(processor)) as GraphElement;
            if (addGraphElement != null && graphElement != null)
            {
                addGraphElement(graphElement, context.screenMousePosition);
                return true;
            }
            return false;
        }
    }
}
