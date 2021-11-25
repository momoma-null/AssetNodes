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
        public IGraphViewCallbackReceiver? GraphViewCallbackReceiver { get; set; }
        public Type graphViewType { get; set; } = typeof(GraphView);

        List<SearchTreeEntry>? m_SearchTree;

        void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }

        void OnDestroy()
        {
            GraphViewCallbackReceiver = null;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            if (m_SearchTree != null)
                return m_SearchTree;
            m_SearchTree = new List<SearchTreeEntry>();
            m_SearchTree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));
            var ctors = NodeProcessorUtility.GetConstructors(graphViewType);
            var nodes = new SortedList<string, Func<INodeProcessor>>(ctors);
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
            if (GraphViewCallbackReceiver != null)
            {
                GraphViewCallbackReceiver.AddElement(new NodeData(processor), context.screenMousePosition);
                return true;
            }
            return false;
        }
    }
}
