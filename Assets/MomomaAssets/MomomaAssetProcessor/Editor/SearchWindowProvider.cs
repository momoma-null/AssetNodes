using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.AssetProcessor
{
    sealed class SearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        public event Action<GraphElement>? addGraphElement;

        void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));
            foreach (var ctor in NodeUtility.Constructors)
            {
                var node = ctor();
                entries.Add(new SearchTreeEntry(new GUIContent(node.Title)) { level = 1, userData = ctor });
            }
            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var iNode = (SearchTreeEntry.userData as Func<INode>)?.Invoke();
            if (iNode == null)
                return false;
            var graphElement = new NodeGUI(iNode);
            if (graphElement == null)
                return false;
            addGraphElement?.Invoke(graphElement);
            return true;
        }
    }
}
