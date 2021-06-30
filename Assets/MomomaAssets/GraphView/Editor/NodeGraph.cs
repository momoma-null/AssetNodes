using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityObject = UnityEngine.Object;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;

    public sealed class NodeGraph : IDisposable, ISelection
    {
        sealed class GraphViewObjectHandler : IDisposable, IEquatable<GraphViewObjectHandler>
        {
            public GraphViewObjectHandler(GraphViewObject graphViewObject, Type graphViewType, Action<string> onGraphElementChanged, Action onGraphViewChanged)
            {
                m_GraphViewObject = graphViewObject;
                m_SerializedObject = new SerializedObject(m_GraphViewObject);
                m_SerializedGraphElementsProperty = m_SerializedObject.FindProperty("m_SerializedGraphElements");
                using (var sp = m_SerializedObject.FindProperty("m_GraphViewTypeName"))
                    sp.stringValue = graphViewType.AssemblyQualifiedName;
                m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                m_OnGraphElementChanged = onGraphElementChanged;
                foreach (var element in m_GraphViewObject.SerializedGraphElements)
                {
                    if (element is GraphElementObject graphElementObject)
                    {
                        graphElementObject.onValueChanged += m_OnGraphElementChanged;
                    }
                }
                m_OnGraphViewChanged = onGraphViewChanged;
                m_GraphViewObject.onValueChanged += RegisterAllValueChangedEvents;
                m_GraphViewObject.onValueChanged += m_OnGraphViewChanged;
            }

            ~GraphViewObjectHandler() => Dispose();

            readonly Action<string> m_OnGraphElementChanged;
            readonly Action m_OnGraphViewChanged;

            SerializedObject m_SerializedObject;
            SerializedProperty m_SerializedGraphElementsProperty;
            GraphViewObject m_GraphViewObject;

            public bool IsValid => m_GraphViewObject != null;
            public IReadOnlyDictionary<string, ISerializedGraphElement> GuidToSerializedGraphElements => m_GraphViewObject.GuidToSerializedGraphElements;

            public void Dispose()
            {
                if (m_GraphViewObject != null)
                {
                    foreach (var element in m_GraphViewObject.SerializedGraphElements)
                    {
                        if (element is GraphElementObject graphElementObject)
                        {
                            graphElementObject.onValueChanged -= m_OnGraphElementChanged;
                        }
                    }
                    m_GraphViewObject.onValueChanged -= RegisterAllValueChangedEvents;
                    m_GraphViewObject.onValueChanged -= m_OnGraphViewChanged;
                }
                m_SerializedGraphElementsProperty.Dispose();
                m_SerializedObject.Dispose();
            }

            public bool Equals(GraphViewObjectHandler other)
            {
                return other.m_GraphViewObject == m_GraphViewObject;
            }

            public void CreateMainAsset(string pathName)
            {
                AssetDatabase.CreateAsset(m_GraphViewObject, pathName);
            }

            void RegisterAllValueChangedEvents()
            {
                foreach (var element in m_GraphViewObject.SerializedGraphElements)
                {
                    if (element is GraphElementObject graphElementObject)
                    {
                        graphElementObject.onValueChanged -= m_OnGraphElementChanged;
                        graphElementObject.onValueChanged += m_OnGraphElementChanged;
                    }
                }
            }

            public sealed class GetScope : IDisposable
            {
                readonly GraphViewObjectHandler m_Handler;

                public GetScope(GraphViewObjectHandler handler)
                {
                    m_Handler = handler;
                    if (!m_Handler.m_SerializedObject.hasModifiedProperties)
                        m_Handler.m_SerializedObject.UpdateIfRequiredOrScript();
                }

                void IDisposable.Dispose() { }

                public GraphElementObject? TryGetGraphElementObjectByGuid(string guid)
                {
                    if (m_Handler.m_GraphViewObject.GuidToIndices.TryGetValue(guid, out var index))
                    {
                        using (var sp = m_Handler.m_SerializedGraphElementsProperty.GetArrayElementAtIndex(index))
                        {
                            return sp.objectReferenceValue as GraphElementObject;
                        }
                    }
                    return null;
                }

            }

            public sealed class SetScope : IDisposable
            {
                readonly GraphViewObjectHandler m_Handler;
                readonly bool m_WithoutUndo;
                readonly HashSet<UnityObject> m_ToDeleteAssets = new HashSet<UnityObject>();
                readonly bool m_IsModifying;

                public SetScope(GraphViewObjectHandler handler, bool withoutUndo = false)
                {
                    m_Handler = handler;
                    m_WithoutUndo = withoutUndo;
                    m_IsModifying = m_Handler.m_SerializedObject.hasModifiedProperties;
                    if (!m_IsModifying)
                        m_Handler.m_SerializedObject.UpdateIfRequiredOrScript();
                }

                void IDisposable.Dispose()
                {
                    EditorApplication.delayCall += DelayDispose;
                    if (!m_IsModifying && m_Handler.m_SerializedObject.hasModifiedProperties)
                    {
                        if (m_WithoutUndo)
                        {
                            m_Handler.m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                        }
                        else
                        {
                            m_Handler.m_SerializedObject.ApplyModifiedProperties();
                        }
                    }
                }

                void DelayDispose()
                {
                    EditorApplication.delayCall -= DelayDispose;
                    if (m_WithoutUndo)
                    {
                        foreach (var i in m_ToDeleteAssets)
                            DestroyImmediate(i, true);
                    }
                    else
                    {
                        foreach (var i in m_ToDeleteAssets)
                            Undo.DestroyObjectImmediate(i);
                    }
                    var path = AssetDatabase.GetAssetPath(m_Handler.m_GraphViewObject);
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.SaveAssets();
                        AssetDatabase.ImportAsset(path);
                    }
                }

                public void AddGraphElementObject(GraphElementObject graphElementObject)
                {
                    var path = AssetDatabase.GetAssetPath(m_Handler.m_GraphViewObject);
                    graphElementObject.hideFlags &= ~HideFlags.DontSaveInEditor;
                    AssetDatabase.AddObjectToAsset(graphElementObject, path);
                    if (!m_WithoutUndo)
                        Undo.RegisterCreatedObjectUndo(graphElementObject, $"Create {nameof(GraphElementObject)}");
                    graphElementObject.onValueChanged += m_Handler.m_OnGraphElementChanged;
                    ++m_Handler.m_SerializedGraphElementsProperty.arraySize;
                    using (var sp = m_Handler.m_SerializedGraphElementsProperty.GetArrayElementAtIndex(m_Handler.m_SerializedGraphElementsProperty.arraySize - 1))
                        sp.objectReferenceValue = graphElementObject;
                    m_ToDeleteAssets.Remove(graphElementObject);
                }

                public void DeleteGraphElementObjects(IEnumerable<string> guids)
                {
                    var indices = new SortedSet<int>();
                    foreach (var guid in guids)
                    {
                        if (m_Handler.m_GraphViewObject.GuidToIndices.TryGetValue(guid, out var index))
                            indices.Add(index);
                    }
                    foreach (var index in indices.Reverse())
                    {
                        using (var sp = m_Handler.m_SerializedGraphElementsProperty.GetArrayElementAtIndex(index))
                        {
                            if (sp.objectReferenceValue is GraphElementObject graphElementObject)
                            {
                                sp.objectReferenceValue = null;
                                sp.DeleteCommand();
                                graphElementObject.onValueChanged -= m_Handler.m_OnGraphElementChanged;
                                m_ToDeleteAssets.Add(graphElementObject);
                            }
                        }
                    }
                }
            }
        }

        readonly DefaultGraphView m_GraphView;
        readonly EditorWindow m_EditorWindow;
        readonly SearchWindowProvider m_SearchWindowProvider;
        readonly VisualElement m_CreateGraphButton;
        readonly Type m_NodeGraphType;

        GraphViewObjectHandler? m_GraphViewObjectHandler = null;
        bool isDisposed = false;

        public event Action? PreProcess;
        public event Action? PostProcess;

        List<ISelectable> ISelection.selection => m_GraphView.selection;

        public NodeGraph(EditorWindow editorWindow)
        {
            if (editorWindow == null)
                throw new ArgumentNullException("editorWindow");
            m_EditorWindow = editorWindow;
            m_NodeGraphType = m_EditorWindow.GetType();
            m_GraphView = new DefaultGraphView(this);
            m_GraphView.style.flexGrow = 1;
            m_EditorWindow.rootVisualElement.Add(m_GraphView);
            m_GraphView.serializeGraphElements = SerializeGraphElements;
            m_GraphView.unserializeAndPaste = UnserializeAndPaste;
            m_GraphView.graphViewChanged = GraphViewChanged;
            m_GraphView.styleSheets.Add(Resources.Load<StyleSheet>("GraphViewStyles"));
            m_GraphView.Insert(0, new GridBackground() { style = { alignItems = Align.Center, justifyContent = Justify.Center } });
            var miniMap = new MiniMap();
            m_GraphView.Add(miniMap);
            miniMap.SetPosition(new Rect(0, 0, miniMap.maxWidth, miniMap.maxHeight));
            m_GraphView.Add(new Button(StartProcess) { text = "Process", style = { alignSelf = Align.FlexEnd } });
            m_GraphView.AddManipulator(new SelectionDragger());
            m_GraphView.AddManipulator(new ContentDragger());
            m_GraphView.AddManipulator(new ContentZoomer());
            m_GraphView.AddManipulator(new RectangleSelector());
            m_GraphView.viewDataKey = Guid.NewGuid().ToString();
            m_SearchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            m_SearchWindowProvider.addGraphElement += AddElement;
            m_SearchWindowProvider.graphViewType = m_NodeGraphType;
            m_GraphView.nodeCreationRequest = CreateNode;
            m_CreateGraphButton = new VisualElement() { style = { position = Position.Absolute, flexDirection = FlexDirection.Row, justifyContent = Justify.Center } };
            m_GraphView.Insert(1, m_CreateGraphButton);
            m_CreateGraphButton.StretchToParentSize();
            m_CreateGraphButton.Add(new Button(CreateGraphObjectAsset) { text = "Create Graph", style = { alignSelf = Align.Center } });
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
            PreProcess = null;
            PostProcess = null;
            SetGraphViewObjectHandler(null);
            DestroyImmediate(m_SearchWindowProvider);
            m_GraphView.graphElements.ForEach(i => { if (i is IDisposable disposable) disposable.Dispose(); });
        }

        void ISelection.AddToSelection(ISelectable selectable)
        {
            if (m_GraphViewObjectHandler != null)
            {
                using (var getScope = new GraphViewObjectHandler.GetScope(m_GraphViewObjectHandler))
                {
                    if (selectable is GraphElement element)
                    {
                        var graphElementObject = getScope.TryGetGraphElementObjectByGuid(element.viewDataKey);
                        if (graphElementObject != null)
                        {
                            var ids = new int[Selection.instanceIDs.Length + 1];
                            Array.Copy(Selection.instanceIDs, ids, Selection.instanceIDs.Length);
                            ids[ids.Length - 1] = graphElementObject.GetInstanceID();
                            Selection.instanceIDs = ids;
                        }
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
                using (var getScope = new GraphViewObjectHandler.GetScope(m_GraphViewObjectHandler))
                {
                    if (selectable is GraphElement element)
                    {
                        var graphElementObject = getScope.TryGetGraphElementObjectByGuid(element.viewDataKey);
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
        }

        void SetGraphViewObjectHandler(GraphViewObjectHandler? graphViewObjectHandler)
        {
            if (m_GraphViewObjectHandler == graphViewObjectHandler)
                return;
            if (m_GraphViewObjectHandler != null)
            {
                if (graphViewObjectHandler != null && m_GraphViewObjectHandler.IsValid && m_GraphViewObjectHandler.Equals(graphViewObjectHandler))
                    return;
                m_GraphViewObjectHandler.Dispose();
            }
            m_GraphViewObjectHandler = graphViewObjectHandler;
        }

        void Update()
        {
            if (m_GraphViewObjectHandler != null && !m_GraphViewObjectHandler.IsValid)
            {
                SetGraphViewObjectHandler(null);
                m_GraphView.DeleteElements(m_GraphView.graphElements.ToList());
                m_CreateGraphButton.visible = true;
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
                        SetGraphViewObjectHandler(new GraphViewObjectHandler(graphViewObject, m_NodeGraphType, OnGraphElementChanged, FullReload));
                        m_CreateGraphButton.visible = false;
                        FullReload();
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
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_SearchWindowProvider);
        }

        void CreateGraphObjectAsset()
        {
            var graphViewObject = ScriptableObject.CreateInstance<GraphViewObject>();
            SetGraphViewObjectHandler(new GraphViewObjectHandler(graphViewObject, m_NodeGraphType, OnGraphElementChanged, FullReload));
            m_CreateGraphButton.visible = false;
            var endAction = ScriptableObject.CreateInstance<CreateGraphObjectEndAction>();
            endAction.OnEndNameEdit += OnEndNameEdit;
            endAction.OnCancelled += OnCreationCancelled;
            var icon = AssetPreview.GetMiniThumbnail(graphViewObject);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(graphViewObject.GetInstanceID(), endAction, "NewNodeGraph.asset", icon, null);
        }

        void OnEndNameEdit(string path)
        {
            if (m_GraphViewObjectHandler == null)
                throw new ArgumentNullException(nameof(m_GraphViewObjectHandler));
            try
            {
                AssetDatabase.StartAssetEditing();
                m_GraphViewObjectHandler.CreateMainAsset(path);
                //m_GraphView.Initialize();
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
            SetGraphViewObjectHandler(null);
            m_CreateGraphButton.visible = true;
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
                        if (edge is IEdgeCallback edgeCallback)
                        {
                            var graphElementObject = CreateGraphElementObject(edge);
                            if (edge.input != null && edge.output != null)
                                graphElementObject.GraphElementData = new DefaultEdgeData(edge.input.viewDataKey, edge.output.viewDataKey);
                            setScope.AddGraphElementObject(graphElementObject);
                            edgeCallback.onPortChanged += OnPortChanged;
                            m_GraphView.AddElement(edge);
                            //m_GraphView.OnValueChanged(edge);
                        }
                        edge.AddManipulator(new ContextualMenuManipulator(context => context.menu.AppendAction("Add Token", action => AddToken(edge, action), action => DropdownMenuAction.Status.Normal)));
                    }
                }
            }
            if (graphViewChange.elementsToRemove != null && graphViewChange.elementsToRemove.Count > 0)
            {
                using (var setScope = new GraphViewObjectHandler.SetScope(m_GraphViewObjectHandler))
                {
                    setScope.DeleteGraphElementObjects(graphViewChange.elementsToRemove.Select(i => i.viewDataKey));
                    foreach (IEdgeCallback e in graphViewChange.elementsToRemove.Where(e => e is IEdgeCallback))
                        e.onPortChanged -= OnPortChanged;
                }
            }
            if (graphViewChange.movedElements != null && graphViewChange.movedElements.Count > 0)
            {
                using (var getScope = new GraphViewObjectHandler.GetScope(m_GraphViewObjectHandler))
                {
                    foreach (var element in graphViewChange.movedElements)
                    {
                        var graphElementObject = getScope.TryGetGraphElementObjectByGuid(element.viewDataKey);
                        if (graphElementObject != null)
                            graphElementObject.Position = element.GetPosition();
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
                var newReferenceGuids = new string[serializedGraphElement.ReferenceGuids.Count];
                for (var i = 0; i < serializedGraphElement.ReferenceGuids.Count; ++i)
                {
                    if (!guidsToReplace.TryGetValue(serializedGraphElement.ReferenceGuids[i], out newGuid))
                    {
                        newGuid = Guid.NewGuid().ToString();
                        guidsToReplace[serializedGraphElement.ReferenceGuids[i]] = newGuid;
                    }
                    newReferenceGuids[i] = newGuid;
                }
                serializedGraphElement.ReferenceGuids = newReferenceGuids;
            }
            var allRect = Rect.zero;
            foreach (var serializedGraphElement in serializedGraphElements)
            {
                var rect = serializedGraphElement.Position;
                if (allRect == Rect.zero)
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
            foreach (var serializedGraphElement in serializedGraphElements.Where(i => !(i.GraphElementData is IEdgeData)))
                serializedGraphElement.Deserialize(m_GraphView);
            foreach (var serializedGraphElement in serializedGraphElements.Where(i => i is IEdgeData))
                serializedGraphElement.Deserialize(m_GraphView);
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
            foreach (var serializedGraphElement in serializedGraphElements.Where(i => !(i.Value.GraphElementData is IEdgeData)))
            {
                if (guids.TryGetValue(serializedGraphElement.Key, out var graphElement))
                    serializedGraphElement.Value.Deserialize(graphElement, m_GraphView);
                else
                    serializedGraphElement.Value.Deserialize(m_GraphView);
            }
            foreach (var serializedGraphElement in serializedGraphElements.Where(i => i.Value.GraphElementData is IEdgeData))
            {
                if (guids.TryGetValue(serializedGraphElement.Key, out var graphElement))
                    serializedGraphElement.Value.Deserialize(graphElement, m_GraphView);
                else
                    serializedGraphElement.Value.Deserialize(m_GraphView);
            }
        }

        void OnPortChanged(Edge edge)
        {
            if (edge.isGhostEdge || m_GraphViewObjectHandler == null)
                return;
            if (edge.input != null && edge.output != null)
            {
                using (var getScope = new GraphViewObjectHandler.GetScope(m_GraphViewObjectHandler))
                {
                    var graphElementObject = getScope.TryGetGraphElementObjectByGuid(edge.viewDataKey);
                    if (graphElementObject != null)
                        graphElementObject.GraphElementData = new DefaultEdgeData(edge.input.viewDataKey, edge.output.viewDataKey);
                }
            }
            //m_GraphView.OnValueChanged(edge);
        }

        void AddToken(Edge edge, DropdownMenuAction action)
        {
            var inputPort = Port.Create<DefaultEdge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, edge.output.portType);
            var outputPort = Port.Create<DefaultEdge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, edge.input.portType);
            var token = new TokenNode<DefaultEdge>(inputPort, outputPort);
            AddElement(token, m_EditorWindow.position.position + action.eventInfo.mousePosition);
            edge.input.Disconnect(edge);
            inputPort.Connect(edge);
            edge.input.ConnectTo<DefaultEdge>(outputPort);
            edge.input = inputPort;
        }

        public void AddElement(GraphElement graphElement, Vector2 screenMousePosition)
        {
            if (m_GraphView.Contains(graphElement))
                throw new UnityException($"{m_GraphView} has already contained {graphElement}.");
            if (m_GraphViewObjectHandler == null)
                throw new ArgumentNullException(nameof(m_GraphViewObjectHandler));
            m_GraphView.AddElement(graphElement);
            var position = Rect.zero;
            var root = m_EditorWindow.rootVisualElement;
            position.center = m_GraphView.contentViewContainer.WorldToLocal(root.ChangeCoordinatesTo(root.parent ?? root, screenMousePosition - m_EditorWindow.position.position));
            graphElement.SetPosition(position);
            var graphElementObject = CreateGraphElementObject(graphElement, position);
            using (var setScope = new GraphViewObjectHandler.SetScope(m_GraphViewObjectHandler))
            {
                setScope.AddGraphElementObject(graphElementObject);
            }
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

        void OnSelectedElementsChanged(List<ISelectable> selection)
        {
            if (m_GraphViewObjectHandler != null)
            {
                var ids = new List<int>(selection.Count);
                using (var getScope = new GraphViewObjectHandler.GetScope(m_GraphViewObjectHandler))
                {
                    foreach (var selectable in selection)
                    {
                        if (selectable is GraphElement element)
                        {
                            var graphElementObject = getScope.TryGetGraphElementObjectByGuid(element.viewDataKey);
                            if (graphElementObject != null)
                                ids.Add(graphElementObject.GetInstanceID());
                        }
                    }
                }
                Selection.instanceIDs = ids.ToArray();
            }
        }

        void OnGraphElementChanged(string guid)
        {
            var element = m_GraphView.GetElementByGuid(guid);
            if (element is IFieldHolder fieldHolder)
                fieldHolder.Update();
            if (m_GraphViewObjectHandler != null && element != null)
            {
                using (var getScope = new GraphViewObjectHandler.GetScope(m_GraphViewObjectHandler))
                {
                    var graphElementObject = getScope.TryGetGraphElementObjectByGuid(guid);
                    if (graphElementObject != null)
                        graphElementObject.Deserialize(element, m_GraphView);
                }
            }
            //m_GraphView.OnValueChanged(element);
        }

        public void StartProcess()
        {
            if (m_GraphViewObjectHandler == null)
                return;
            var portToNode = new Dictionary<string, string>();
            foreach (var i in m_GraphViewObjectHandler.GuidToSerializedGraphElements.Values)
            {
                if (i.GraphElementData is INodeData nodeData)
                {
                    foreach (var port in nodeData.OutputPorts)
                    {
                        portToNode[port.Id] = i.Guid;
                    }
                }
            }
            var inputToPreNodes = new Dictionary<string, HashSet<string>>();
            var outputToInputs = new Dictionary<string, HashSet<string>>();
            var connectedOutputPorts = new HashSet<string>();
            foreach (var i in m_GraphViewObjectHandler.GuidToSerializedGraphElements.Values)
            {
                if (i.GraphElementData is IEdgeData edgeData)
                {
                    if (!inputToPreNodes.TryGetValue(edgeData.InputPortGuid, out var preNodes))
                    {
                        preNodes = new HashSet<string>();
                        inputToPreNodes.Add(edgeData.InputPortGuid, preNodes);
                    }
                    preNodes.Add(portToNode[edgeData.OutputPortGuid]);
                    connectedOutputPorts.Add(edgeData.OutputPortGuid);
                    if (!outputToInputs.TryGetValue(edgeData.OutputPortGuid, out var inputs))
                    {
                        inputs = new HashSet<string>();
                        outputToInputs.Add(edgeData.OutputPortGuid, inputs);
                    }
                    inputs.Add(edgeData.InputPortGuid);
                }
            }
            var endNodes = new HashSet<string>();
            foreach (var i in m_GraphViewObjectHandler.GuidToSerializedGraphElements.Values)
            {
                if (i.GraphElementData is INodeData nodeData)
                {
                    var isEndNode = true;
                    foreach (var port in nodeData.OutputPorts)
                    {
                        if (connectedOutputPorts.Contains(port.Id))
                        {
                            isEndNode = false;
                            break;
                        }
                    }
                    if (isEndNode)
                        endNodes.Add(i.Guid);
                }
            }
            var container = new ProcessingDataContainer(GetData, inputToPreNodes, outputToInputs);
            foreach (var nordId in endNodes)
            {
                GetData(nordId, container);
            }
        }

        void GetData(string id, ProcessingDataContainer container)
        {
            if (m_GraphViewObjectHandler?.GuidToSerializedGraphElements[id].GraphElementData is INodeData nodeData)
            {
                PreProcess?.Invoke();
                nodeData.Process(container);
                PostProcess?.Invoke();
            }
        }
    }
}
