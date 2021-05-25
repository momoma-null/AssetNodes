using System;
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
            entries.Add(new SearchTreeEntry(new GUIContent("Test")) { level = 1, userData = typeof(NodeGUI) });
            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var graphElement = Activator.CreateInstance(SearchTreeEntry.userData as Type, true) as GraphElement;
            if (graphElement == null)
                return false;
            addGraphElement?.Invoke(graphElement);
            return true;
        }
    }
}
