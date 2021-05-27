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
    sealed class NodeGraph<TGraphView> : IDisposable where TGraphView : GraphView
    {
        readonly EditorWindow m_Window;
        readonly SearchWindowProvider m_SearchWindowProvider;
        readonly TGraphView m_GraphView;

        public NodeGraph(EditorWindow window, TGraphView graphview)
        {
            m_Window = window;
            m_GraphView = graphview;
            m_Window.rootVisualElement.Add(m_GraphView);
            m_GraphView.styleSheets.Add(Resources.Load<StyleSheet>("GraphViewStyles"));
            m_GraphView.style.flexGrow = 1;
            m_GraphView.AddManipulator(new ContentZoomer());
            m_GraphView.AddManipulator(new ContentDragger());
            m_GraphView.AddManipulator(new SelectionDragger());
            m_GraphView.AddManipulator(new RectangleSelector());
            m_GraphView.Insert(0, new GridBackground() { style = { alignItems = Align.Center, justifyContent = Justify.Center } });
            var minimap = new MiniMap();
            m_GraphView.Add(minimap);
            minimap.SetPosition(new Rect(0, 0, minimap.maxWidth, minimap.maxHeight));
            m_GraphView.viewDataKey = Guid.NewGuid().ToString();
            m_SearchWindowProvider = CreateInstance<SearchWindowProvider>();
            m_SearchWindowProvider.addGraphElement += m_GraphView.AddElement;
            m_GraphView.nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_SearchWindowProvider);
        }

        public void Dispose()
        {
            DestroyImmediate(m_SearchWindowProvider);
        }
    }
}
