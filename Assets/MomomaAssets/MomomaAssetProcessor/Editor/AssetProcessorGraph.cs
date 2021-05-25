using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using static UnityEngine.Object;
using static UnityEngine.ScriptableObject;

#nullable enable

namespace MomomaAssets.AssetProcessor
{
    sealed class AssetProcessorGraph : GraphView, IDisposable
    {
        readonly EditorWindow m_Window;
        readonly SearchWindowProvider m_SearchWindowProvider;

        public AssetProcessorGraph(EditorWindow window)
        {
            m_Window = window;
            style.flexGrow = 1;
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            Insert(0, new GridBackground() { style = { alignItems = Align.Center, justifyContent = Justify.Center } });
            Add(new MiniMap());
            viewDataKey = Guid.NewGuid().ToString();
            m_SearchWindowProvider = CreateInstance<SearchWindowProvider>();
            m_SearchWindowProvider.addGraphElement += AddElement;
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_SearchWindowProvider);
        }

        public void Dispose()
        {
            DestroyImmediate(m_SearchWindowProvider);
        }
    }
}
