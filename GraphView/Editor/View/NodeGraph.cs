using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class NodeGraph : IDisposable, ISelection, IGraphViewCallbackReceiver
    {
        readonly DefaultGraphView m_GraphView;
        readonly EditorWindow m_EditorWindow;
        readonly SearchWindowProvider m_SearchWindowProvider;
        readonly VisualElement m_CreateGraphButton;
        readonly VisualElement m_StartProcessButton;
        readonly Type m_NodeGraphType;
        readonly NodeGraphProcessor m_NodeGraphProcessor;
        readonly NodeGraphEditorData m_NodeGraphEditorData;

        GraphViewObjectHandler? m_GraphViewObjectHandler = null;
        bool isDisposed = false;
        VisualElement? insertTarget;

        List<ISelectable> ISelection.selection => m_GraphView.selection;

        public NodeGraph(EditorWindow editorWindow, NodeGraphProcessor nodeGraphProcessor, NodeGraphEditorData nodeGraphEditorData)
        {
            m_EditorWindow = editorWindow;
            m_NodeGraphProcessor = nodeGraphProcessor;
            m_NodeGraphEditorData = nodeGraphEditorData;
            m_NodeGraphType = m_EditorWindow.GetType();
            m_GraphView = new DefaultGraphView(this, this);
            m_GraphView.style.flexGrow = 1;
            m_EditorWindow.rootVisualElement.Add(m_GraphView);
            m_GraphView.serializeGraphElements = SerializeGraphElements;
            m_GraphView.unserializeAndPaste = UnserializeAndPaste;
            m_GraphView.graphViewChanged = GraphViewChanged;
            m_GraphView.elementsAddedToGroup = OnElementsAddedToGroup;
            m_GraphView.elementsRemovedFromGroup = OnElementsRemovedFromGroup;
            m_GraphView.elementsInsertedToStackNode = OnElementsInsertedToStackNode;
            m_GraphView.elementsRemovedFromStackNode = OnElementsRemovedFromStackNode;
            m_GraphView.styleSheets.Add(Resources.Load<StyleSheet>("GraphViewStyles"));
            m_GraphView.Insert(0, new GridBackground() { style = { alignItems = Align.Center, justifyContent = Justify.Center } });
            var miniMap = new MiniMap();
            m_GraphView.Add(miniMap);
            miniMap.SetPosition(new Rect(0, 0, miniMap.maxWidth, miniMap.maxHeight));
            m_StartProcessButton = new Button(StartProcess) { text = "Process", style = { alignSelf = Align.FlexEnd } };
            m_GraphView.Add(m_StartProcessButton);
            m_GraphView.AddManipulator(new SelectionDragger());
            m_GraphView.AddManipulator(new ContentDragger());
            m_GraphView.AddManipulator(new ContentZoomer());
            m_GraphView.AddManipulator(new RectangleSelector());
            m_GraphView.viewDataKey = m_NodeGraphEditorData.m_ViewDataKey;
            m_SearchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            m_SearchWindowProvider.GraphViewCallbackReceiver = this;
            m_SearchWindowProvider.graphViewType = m_NodeGraphType;
            m_GraphView.nodeCreationRequest = CreateNode;
            m_CreateGraphButton = new VisualElement() { style = { position = Position.Absolute, flexDirection = FlexDirection.Row, justifyContent = Justify.Center } };
            m_GraphView.Insert(1, m_CreateGraphButton);
            m_CreateGraphButton.StretchToParentSize();
            m_CreateGraphButton.Add(new Button(CreateGraphObjectAsset) { text = "Create Graph", style = { alignSelf = Align.Center } });
            CreateGraphViewObjectHandler(m_NodeGraphEditorData.m_SelectedGraphViewObject);
            OnSelectionChanged();
            EditorApplication.update += Update;
            Selection.selectionChanged += OnSelectionChanged;
        }

        ~NodeGraph() => Dispose();

        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            EditorApplication.update -= Update;
            Selection.selectionChanged -= OnSelectionChanged;
            DestroyImmediate(m_SearchWindowProvider);
        }

        void ISelection.AddToSelection(ISelectable selectable)
        {
            if (m_GraphViewObjectHandler != null)
            {
                if (selectable is GraphElement element)
                {
                    var graphElementObject = m_GraphViewObjectHandler.TryGetGraphElementObjectByGuid(element.viewDataKey);
                    if (graphElementObject != null && !Selection.Contains(graphElementObject))
                    {
                        var ids = new int[Selection.instanceIDs.Length + 1];
                        Array.Copy(Selection.instanceIDs, ids, Selection.instanceIDs.Length);
                        ids[ids.Length - 1] = graphElementObject.GetInstanceID();
                        Selection.instanceIDs = ids;
                    }
                }
            }
        }

        void ISelection.ClearSelection()
        {
            Selection.instanceIDs = new int[0];
        }

        void ISelection.RemoveFromSelection(ISelectable selectable)
        {
            if (m_GraphViewObjectHandler != null)
            {
                if (selectable is GraphElement element)
                {
                    var graphElementObject = m_GraphViewObjectHandler.TryGetGraphElementObjectByGuid(element.viewDataKey);
                    if (graphElementObject != null)
                    {
                        var idSet = new HashSet<int>(Selection.instanceIDs);
                        idSet.Remove(graphElementObject.GetInstanceID());
                        var ids = new int[idSet.Count];
                        idSet.CopyTo(ids);
                        Selection.instanceIDs = ids;
                    }
                }
            }
        }

        void CreateGraphViewObjectHandler(GraphViewObject? graphViewObject)
        {
            if (!isDisposed)
                m_NodeGraphEditorData.m_SelectedGraphViewObject = graphViewObject;
            if (graphViewObject == null)
            {
                m_GraphViewObjectHandler?.Dispose();
                m_GraphViewObjectHandler = null;
                m_GraphView.DeleteElements(m_GraphView.graphElements.ToList());
                m_CreateGraphButton.visible = true;
                m_StartProcessButton.visible = false;
                return;
            }
            var graphViewObjectHandler = new GraphViewObjectHandler(graphViewObject, m_NodeGraphType, OnGraphElementChanged, FullReload, m_NodeGraphProcessor);
            if (m_GraphViewObjectHandler != null)
            {
                if (graphViewObjectHandler != null && m_GraphViewObjectHandler.IsValid && m_GraphViewObjectHandler.Equals(graphViewObjectHandler))
                    return;
                m_GraphViewObjectHandler.Dispose();
            }
            m_GraphViewObjectHandler = graphViewObjectHandler;
            m_CreateGraphButton.visible = false;
            m_StartProcessButton.visible = true;
            FullReload();
        }

        void Update()
        {
            if (m_GraphViewObjectHandler != null && !m_GraphViewObjectHandler.IsValid)
            {
                CreateGraphViewObjectHandler(null);
            }
        }

        void OnSelectionChanged()
        {
            var doClearSelection = true;
            foreach (var obj in Selection.objects)
            {
                if (obj is GraphViewObject graphViewObject)
                {
                    if (graphViewObject.GraphViewType == m_NodeGraphType)
                    {
                        CreateGraphViewObjectHandler(graphViewObject);
                    }
                }
                else if (obj is GraphElementObject)
                {
                    doClearSelection = false;
                }
            }
            if (doClearSelection)
                m_GraphView.ClearSelection();
        }

        void CreateNode(NodeCreationContext context)
        {
            insertTarget = context.target;
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_SearchWindowProvider);
        }

        void CreateGraphObjectAsset()
        {
            var graphViewObject = ScriptableObject.CreateInstance<GraphViewObject>();
            CreateGraphViewObjectHandler(graphViewObject);
            var endNameEditCallback = ScriptableObject.CreateInstance<EndNameEditCallback>();
            endNameEditCallback.OnEndNameEdit += OnEndNameEdit;
            endNameEditCallback.OnCancelled += OnCreationCancelled;
            var icon = AssetPreview.GetMiniThumbnail(graphViewObject);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(graphViewObject.GetInstanceID(), endNameEditCallback, "NewNodeGraph.asset", icon, null);
        }

        void OnEndNameEdit(string path)
        {
            if (m_GraphViewObjectHandler == null)
                throw new ArgumentNullException(nameof(m_GraphViewObjectHandler));
            try
            {
                AssetDatabase.StartAssetEditing();
                m_GraphViewObjectHandler.CreateMainAsset(path);
                using (var setScope = new GraphViewObjectHandler.SetScope(m_GraphViewObjectHandler, true))
                {
                    foreach (var graphElement in m_GraphView.graphElements.ToList())
                    {
                        var graphElementObject = CreateGraphElementObject(graphElement);
                        setScope.AddGraphElementObject(graphElementObject);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadMainAssetAtPath(path));
        }

        void OnCreationCancelled()
        {
            CreateGraphViewObjectHandler(null);
        }

        void OnElementsAddedToGroup(Group group, IEnumerable<GraphElement> elements)
        {
            if (m_GraphViewObjectHandler == null)
                return;
            var graphElementObject = m_GraphViewObjectHandler.TryGetGraphElementObjectByGuid(group.viewDataKey);
            if (graphElementObject != null && graphElementObject.GraphElementData is IGroupData groupData)
            {
                Undo.RecordObject(graphElementObject, "Add elements to group");
                groupData.AddElements(elements.Select(i => i.viewDataKey));
            }
        }

        void OnElementsRemovedFromGroup(Group group, IEnumerable<GraphElement> elements)
        {
            if (m_GraphViewObjectHandler == null)
                return;
            var graphElementObject = m_GraphViewObjectHandler.TryGetGraphElementObjectByGuid(group.viewDataKey);
            if (graphElementObject != null && graphElementObject.GraphElementData is IGroupData groupData)
            {
                Undo.RecordObject(graphElementObject, "Remove elements from group");
                groupData.RemoveElements(elements.Select(i => i.viewDataKey));
                if (groupData.ElementCount == 0)
                    m_GraphView.RemoveElement(group);
            }
        }

        void OnElementsInsertedToStackNode(StackNode stackNode, int index, IEnumerable<GraphElement> elements)
        {
            if (m_GraphViewObjectHandler == null)
                return;
            var graphElementObject = m_GraphViewObjectHandler.TryGetGraphElementObjectByGuid(stackNode.viewDataKey);
            if (graphElementObject != null && graphElementObject.GraphElementData is IStackNodeData stackNodeData)
            {
                Undo.RecordObject(graphElementObject, "Insert elements to stack node");
                stackNodeData.InsertElements(index, elements.Select(i => i.viewDataKey));
            }
        }

        void OnElementsRemovedFromStackNode(StackNode stackNode, IEnumerable<GraphElement> elements)
        {
            if (m_GraphViewObjectHandler == null)
                return;
            var graphElementObject = m_GraphViewObjectHandler.TryGetGraphElementObjectByGuid(stackNode.viewDataKey);
            if (graphElementObject != null && graphElementObject.GraphElementData is IStackNodeData stackNodeData)
            {
                Undo.RecordObject(graphElementObject, "Remove elements from stack node");
                stackNodeData.RemoveElements(elements.Select(i => i.viewDataKey));
            }
        }

        GraphViewChange GraphViewChanged(GraphViewChange graphViewChange)
        {
            if (m_GraphViewObjectHandler == null)
                return graphViewChange;
            if (graphViewChange.edgesToCreate != null && graphViewChange.edgesToCreate.Count > 0)
            {
                using (var setScope = new GraphViewObjectHandler.SetScope(m_GraphViewObjectHandler))
                {
                    foreach (var edge in graphViewChange.edgesToCreate)
                    {
                        var graphElementObject = CreateGraphElementObject(edge);
                        setScope.AddGraphElementObject(graphElementObject);
                    }
                }
            }
            if (graphViewChange.elementsToRemove != null && graphViewChange.elementsToRemove.Count > 0)
            {
                using (var setScope = new GraphViewObjectHandler.SetScope(m_GraphViewObjectHandler))
                {
                    setScope.DeleteGraphElementObjects(new HashSet<string>(graphViewChange.elementsToRemove.Select(i => i.viewDataKey)));
                }
            }
            if (graphViewChange.movedElements != null && graphViewChange.movedElements.Count > 0)
            {
                foreach (var element in graphViewChange.movedElements)
                {
                    if (m_GraphViewObjectHandler.GuidToSerializedGraphElements.TryGetValue(element.viewDataKey, out var serializedGraphElement))
                        serializedGraphElement.Position = element.GetPosition();
                    if (element is Scope scope)
                    {
                        foreach (var e in scope.containedElements)
                        {
                            if (m_GraphViewObjectHandler.GuidToSerializedGraphElements.TryGetValue(e.viewDataKey, out var child))
                            {
                                var rect = child.Position;
                                rect.position += graphViewChange.moveDelta;
                                child.Position = rect;
                            }
                        }
                    }
                }
            }
            return graphViewChange;
        }

        string SerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            var serializedGraphElements = new List<SerializedGraphElement>();
            foreach (var element in elements)
            {
                var serializedGraphElement = new SerializedGraphElement();
                element.Serialize(serializedGraphElement, element.GetPosition());
                serializedGraphElements.Add(serializedGraphElement);
            }
            var serializedGraphView = new SerializedGraphView(serializedGraphElements);
            return EditorJsonUtility.ToJson(serializedGraphView);
        }

        void UnserializeAndPaste(string operationName, string data)
        {
            var serializedGraphView = new SerializedGraphView(new List<SerializedGraphElement>());
            EditorJsonUtility.FromJsonOverwrite(data, serializedGraphView);
            var serializedGraphElements = serializedGraphView.SerializedGraphElements;
            var guidsToReplace = new Dictionary<string, string>();
            foreach (var serializedGraphElement in serializedGraphElements)
            {
                if (!guidsToReplace.TryGetValue(serializedGraphElement.Guid, out var newGuid))
                {
                    newGuid = Guid.NewGuid().ToString();
                    guidsToReplace[serializedGraphElement.Guid] = newGuid;
                }
                serializedGraphElement.Guid = newGuid;
                serializedGraphElement.GraphElementData?.ReplaceGuid(guidsToReplace);
            }
            var allRect = Rect.zero;
            foreach (var serializedGraphElement in serializedGraphElements)
            {
                var rect = serializedGraphElement.Position;
                if (rect.size == Vector2.zero)
                    continue;
                if (allRect.size == Vector2.zero)
                    allRect = rect;
                else
                {
                    var xMax = Math.Max(rect.xMax, allRect.xMax);
                    var xMin = Math.Min(rect.xMin, allRect.xMin);
                    var yMax = Math.Max(rect.yMax, allRect.yMax);
                    var yMin = Math.Min(rect.yMin, allRect.yMin);
                    allRect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
                }
            }
            var offset = m_GraphView.contentViewContainer.WorldToLocal(m_GraphView.contentRect.center) - allRect.center;
            foreach (var serializedGraphElement in serializedGraphElements)
            {
                var rect = serializedGraphElement.Position;
                rect.position += offset;
                serializedGraphElement.Position = rect;
            }
            foreach (var serializedGraphElement in serializedGraphElements.OrderBy(i => i.GraphElementData?.Priority ?? 0))
            {
                serializedGraphElement.Deserialize(m_GraphView);
                if (serializedGraphElement.GraphElementData is IAdditionalAssetHolder assetHolder)
                    assetHolder.OnClone();
            }
            if (m_GraphViewObjectHandler != null)
            {
                using (var setScope = new GraphViewObjectHandler.SetScope(m_GraphViewObjectHandler))
                {
                    var guids = new Dictionary<string, GraphElement>();
                    m_GraphView.graphElements.ForEach(i => guids.Add(i.viewDataKey, i));
                    foreach (var serializedGraphElement in serializedGraphElements)
                    {
                        if (guids.TryGetValue(serializedGraphElement.Guid, out var graphElement))
                        {
                            var graphElementObject = CreateGraphElementObject(graphElement, serializedGraphElement.Position);
                            setScope.AddGraphElementObject(graphElementObject);
                        }
                    }
                }
            }
        }

        void FullReload()
        {
            if (m_GraphViewObjectHandler == null)
                return;
            var serializedGraphElements = m_GraphViewObjectHandler.GuidToSerializedGraphElements;
            var elementsToRemove = new HashSet<GraphElement>(m_GraphView.graphElements.ToList());
            elementsToRemove.RemoveWhere(element => serializedGraphElements.ContainsKey(element.viewDataKey));
            m_GraphView.DeleteElements(elementsToRemove);
            var guids = m_GraphView.graphElements.ToList().ToDictionary(element => element.viewDataKey, element => element);
            foreach (var serializedGraphElement in serializedGraphElements.OrderBy(i => i.Value.GraphElementData?.Priority ?? 0))
            {
                var guid = serializedGraphElement.Key;
                if (guids.TryGetValue(guid, out var graphElement))
                    serializedGraphElement.Value.Deserialize(graphElement, m_GraphView);
                else
                    serializedGraphElement.Value.Deserialize(m_GraphView);
            }
        }

        void IGraphViewCallbackReceiver.AddElement(IGraphElementData graphElementData, Vector2 screenMousePosition)
        {
            if (m_GraphViewObjectHandler == null)
                throw new ArgumentNullException(nameof(m_GraphViewObjectHandler));
            var position = Rect.zero;
            var root = m_EditorWindow.rootVisualElement;
            position.center = m_GraphView.contentViewContainer.WorldToLocal(root.ChangeCoordinatesTo(root.parent ?? root, screenMousePosition - m_EditorWindow.position.position));
            var serializedGraphElement = new SerializedGraphElement()
            {
                Position = position,
                GraphElementData = graphElementData
            };
            var graphElement = serializedGraphElement.Deserialize(m_GraphView);
            var graphElementObject = CreateGraphElementObject(graphElement, position);
            using (var setScope = new GraphViewObjectHandler.SetScope(m_GraphViewObjectHandler))
            {
                setScope.AddGraphElementObject(graphElementObject);
            }
            if (insertTarget is Port port && graphElement is Node node)
            {
                var ports = port.direction == Direction.Input ? node.outputContainer.Query<Port>().ToList() : node.inputContainer.Query<Port>().ToList();
                foreach (var dst in ports)
                {
                    if (m_GraphView.CanConnectPortType(port, dst))
                    {
                        var edge = port.ConnectTo<BindableEdge>(dst);
                        edge.SetPosition(Rect.zero);
                        EdgeConnectorListener.Default.OnDrop(m_GraphView, edge);
                        break;
                    }
                }
            }
            insertTarget = null;
        }

        GraphElementObject CreateGraphElementObject(GraphElement graphElement)
        {
            return CreateGraphElementObject(graphElement, graphElement.GetPosition());
        }

        GraphElementObject CreateGraphElementObject(GraphElement graphElement, Rect position)
        {
            var graphElementObject = ScriptableObject.CreateInstance<GraphElementObject>();
            graphElement.Serialize(graphElementObject, position);
            return graphElementObject;
        }

        void OnGraphElementChanged(ISerializedGraphElement serializedGraphElement)
        {
            var element = m_GraphView.GetElementByGuid(serializedGraphElement.Guid);
            if (m_GraphViewObjectHandler != null && element != null)
                serializedGraphElement.Deserialize(element, m_GraphView);
        }

        public void StartProcess()
        {
            m_GraphViewObjectHandler?.StartProcess();
        }
    }
}
