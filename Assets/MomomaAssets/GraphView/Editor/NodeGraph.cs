﻿using System;
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

    public sealed class NodeGraph<TGraphView, TEdge> : IDisposable
        where TGraphView : GraphView, IGraphViewCallback
        where TEdge : Edge, IEdgeCallback, new()
    {
        sealed class GraphViewObjectHandler : IDisposable
        {
            public GraphViewObjectHandler(GraphViewObject graphViewObject, Type graphViewType, Action<string> onValueChanged)
            {
                m_GraphViewObject = graphViewObject;
                m_SerializedObject = new SerializedObject(m_GraphViewObject);
                m_SerializedGraphElementsProperty = m_SerializedObject.FindProperty("m_SerializedGraphElements");
                using (var sp = m_SerializedObject.FindProperty("m_GraphViewTypeName"))
                    sp.stringValue = graphViewType.AssemblyQualifiedName;
                m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                this.onValueChanged = onValueChanged;
                foreach (var element in m_GraphViewObject.SerializedGraphElements)
                {
                    if (element is GraphElementObject graphElementObject)
                    {
                        graphElementObject.onValueChanged += onValueChanged;
                    }
                }
            }

            ~GraphViewObjectHandler() { Dispose(); }

            readonly Action<string> onValueChanged;

            public bool CanFullReload => IsValid && m_SerializedObject.UpdateIfRequiredOrScript();
            public GraphViewObject GraphViewObject => m_GraphViewObject;

            public bool IsValid => m_SerializedObject.targetObject != null;

            SerializedObject m_SerializedObject;
            SerializedProperty m_SerializedGraphElementsProperty;
            GraphViewObject m_GraphViewObject;

            public void Dispose()
            {
                if (m_GraphViewObject != null)
                {
                    foreach (var element in m_GraphViewObject.SerializedGraphElements)
                    {
                        if (element is GraphElementObject graphElementObject)
                        {
                            graphElementObject.onValueChanged -= onValueChanged;
                        }
                    }
                }
                m_SerializedGraphElementsProperty.Dispose();
                m_SerializedObject.Dispose();
            }

            public void Update()
            {
                m_SerializedObject.Update();
            }

            public bool ApplyModifiedProperties()
            {
                return m_SerializedObject.ApplyModifiedProperties();
            }

            public bool ApplyModifiedPropertiesWithoutUndo()
            {
                return m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            public void CreateMainAsset(string pathName)
            {
                GraphViewObject.hideFlags &= ~HideFlags.DontSaveInEditor;
                AssetDatabase.CreateAsset(GraphViewObject, pathName);
                GraphViewObject.hideFlags |= HideFlags.DontSaveInEditor;
            }

            public void AddGraphElementObject(GraphElementObject graphElementObject)
            {
                graphElementObject.hideFlags &= ~HideFlags.DontSaveInEditor;
                AssetDatabase.AddObjectToAsset(graphElementObject, AssetDatabase.GetAssetPath(GraphViewObject));
                graphElementObject.hideFlags |= HideFlags.DontSaveInEditor;
                graphElementObject.onValueChanged += onValueChanged;
                ++m_SerializedGraphElementsProperty.arraySize;
                using (var sp = m_SerializedGraphElementsProperty.GetArrayElementAtIndex(m_SerializedGraphElementsProperty.arraySize - 1))
                    sp.objectReferenceValue = graphElementObject;
            }

            public GraphElementObject DeleteGraphElementObjectAtIndex(int index)
            {
                using (var sp = m_SerializedGraphElementsProperty.GetArrayElementAtIndex(index))
                {
                    if (sp.objectReferenceValue is GraphElementObject graphElementObject)
                    {
                        sp.objectReferenceValue = null;
                        sp.DeleteCommand();
                        graphElementObject.hideFlags &= ~HideFlags.DontSaveInEditor;
                        AssetDatabase.RemoveObjectFromAsset(graphElementObject);
                        graphElementObject.onValueChanged -= onValueChanged;
                        return graphElementObject;
                    }
                    throw new InvalidOperationException();
                }
            }

            public GraphElementObject GetGraphElementObjectAtIndex(int index)
            {
                using (var sp = m_SerializedGraphElementsProperty.GetArrayElementAtIndex(index))
                {
                    if (sp.objectReferenceValue is GraphElementObject graphElementObject)
                        return graphElementObject;
                    throw new InvalidOperationException();
                }
            }
        }

        readonly TGraphView m_GraphView;
        readonly EditorWindow m_EditorWindow;
        readonly SearchWindowProvider m_SearchWindowProvider;
        readonly VisualElement m_CreateGraphButton;

        GraphViewObjectHandler? m_GraphViewObjectHandler = null;
        GraphViewObjectHandler Handler
        {
            get
            {
                if (m_GraphViewObjectHandler != null)
                    return m_GraphViewObjectHandler;
                throw new NullReferenceException("m_GraphViewObjectHandler does not exist");
            }
            set
            {
                if (m_GraphViewObjectHandler == value)
                    return;
                if (m_GraphViewObjectHandler != null)
                {
                    if (m_GraphViewObjectHandler.GraphViewObject == value.GraphViewObject)
                        return;
                    m_GraphViewObjectHandler.Dispose();
                }
                m_GraphViewObjectHandler = value;
            }
        }

        bool isDisposed = false;

        public NodeGraph(EditorWindow editorWindow, TGraphView graphView)
        {
            if (editorWindow == null)
                throw new ArgumentNullException("editorWindow");
            m_EditorWindow = editorWindow;
            m_GraphView = graphView;
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
            m_GraphView.AddManipulator(new SelectionDragger());
            m_GraphView.AddManipulator(new ContentDragger());
            m_GraphView.AddManipulator(new ContentZoomer());
            m_GraphView.AddManipulator(new RectangleSelector());
            m_GraphView.onSelectionChanged += OnSelectedElementsChanged;
            m_GraphView.viewDataKey = Guid.NewGuid().ToString();
            m_SearchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            m_SearchWindowProvider.addGraphElement += AddElement;
            m_SearchWindowProvider.graphViewType = typeof(TGraphView);
            m_GraphView.nodeCreationRequest = CreateNode;
            m_CreateGraphButton = new VisualElement() { style = { position = Position.Absolute, flexDirection = FlexDirection.Row, justifyContent = Justify.Center } };
            m_GraphView.Insert(1, m_CreateGraphButton);
            m_CreateGraphButton.StretchToParentSize();
            m_CreateGraphButton.Add(new Button(CreateGraphObjectAsset) { text = "Create Graph", style = { alignSelf = Align.Center } });
            OnSelectionChanged();
            Undo.undoRedoPerformed += FullReload;
            EditorApplication.update += Update;
            Selection.selectionChanged += OnSelectionChanged;
        }

        ~NodeGraph() => Dispose();

        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            Undo.undoRedoPerformed -= FullReload;
            EditorApplication.update -= Update;
            Selection.selectionChanged -= OnSelectionChanged;
            m_GraphViewObjectHandler?.Dispose();
            m_GraphViewObjectHandler = null;
            DestroyImmediate(m_SearchWindowProvider);
            if (m_GraphView is IDisposable disposable)
                disposable.Dispose();
        }

        void Update()
        {
            if (m_GraphViewObjectHandler != null && !m_GraphViewObjectHandler.IsValid)
            {
                m_GraphView.DeleteElements(m_GraphView.graphElements.ToList());
                m_GraphViewObjectHandler?.Dispose();
                m_GraphViewObjectHandler = null;
                m_CreateGraphButton.visible = true;
            }
        }

        void OnSelectionChanged()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is GraphViewObject graphViewObject)
                {
                    if (graphViewObject.GraphViewType == typeof(TGraphView))
                    {
                        Handler = new GraphViewObjectHandler(graphViewObject, typeof(TGraphView), OnValueChanged);
                        m_CreateGraphButton.visible = false;
                        FullReload();
                        Debug.Log(m_GraphViewObjectHandler);
                    }
                }
            }
        }

        void CreateNode(NodeCreationContext context)
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_SearchWindowProvider);
        }

        void CreateGraphObjectAsset()
        {
            var graphViewObject = ScriptableObject.CreateInstance<GraphViewObject>();
            Handler = new GraphViewObjectHandler(graphViewObject, typeof(TGraphView), OnValueChanged);
            m_CreateGraphButton.visible = false;
            var endAction = ScriptableObject.CreateInstance<CreateGraphObjectEndAction>();
            endAction.OnEndNameEdit += OnEndNameEdit;
            endAction.OnCancelled += OnCreationCancelled;
            var icon = AssetPreview.GetMiniThumbnail(graphViewObject);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(graphViewObject.GetInstanceID(), endAction, "NewTextureGraph.asset", icon, null);
        }

        void OnEndNameEdit(string pathName)
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                Handler.CreateMainAsset(pathName);
                m_GraphView.Initialize();
                Handler.Update();
                foreach (var graphElement in m_GraphView.graphElements.ToList())
                {
                    var graphElementObject = CreateGraphElementObject(graphElement, true);
                    Handler.AddGraphElementObject(graphElementObject);
                }
                Handler.ApplyModifiedPropertiesWithoutUndo();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            AssetDatabase.ImportAsset(pathName);
            AssetDatabase.SaveAssets();
            ProjectWindowUtil.ShowCreatedAsset(Handler.GraphViewObject);
        }

        void OnCreationCancelled()
        {
            m_GraphViewObjectHandler?.Dispose();
            m_GraphViewObjectHandler = null;
            m_CreateGraphButton.visible = true;
        }

        GraphViewChange GraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.edgesToCreate != null)
            {
                Handler.Update();
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    if (edge is IEdgeCallback edgeCallback)
                    {
                        var graphElementObject = CreateGraphElementObject(edge);
                        Handler.AddGraphElementObject(graphElementObject);
                        edgeCallback.onPortChanged -= OnPortChanged;
                        edgeCallback.onPortChanged += OnPortChanged;
                        m_GraphView.OnValueChanged(edge);
                    }
                    edge.AddManipulator(new ContextualMenuManipulator(context => context.menu.AppendAction("Add Token", action => AddToken(edge, action), action => DropdownMenuAction.Status.Normal)));
                }
                Handler.ApplyModifiedProperties();
            }
            if (graphViewChange.elementsToRemove != null)
            {
                Handler.Update();
                var indicesToRemove = new List<int>();
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (Handler.GraphViewObject.GuidToIndices.TryGetValue(element.viewDataKey, out var index))
                        indicesToRemove.Add(index);
                }
                indicesToRemove.Sort();
                var objectsToDelete = new List<UnityObject>();
                for (var i = indicesToRemove.Count - 1; i > -1; --i)
                {
                    objectsToDelete.Add(Handler.DeleteGraphElementObjectAtIndex(indicesToRemove[i]));
                }
                Handler.ApplyModifiedProperties();
                foreach (var obj in objectsToDelete)
                    Undo.DestroyObjectImmediate(obj);
            }
            if (graphViewChange.movedElements != null)
            {
                foreach (var element in graphViewChange.movedElements)
                {
                    if (Handler.GraphViewObject.GuidToIndices.TryGetValue(element.viewDataKey, out var index))
                        Handler.GetGraphElementObjectAtIndex(index).Position = element.GetPosition();
                }
            }
            return graphViewChange;
        }

        string SerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            var serializedGraphView = new SerializedGraphView();
            foreach (var element in elements)
            {
                var serializedGraphElement = new SerializedGraphElement();
                element.Serialize(serializedGraphElement);
                serializedGraphView.SerializedGraphElements.Add(serializedGraphElement);
            }
            return EditorJsonUtility.ToJson(serializedGraphView);
        }

        void UnserializeAndPaste(string operationName, string data)
        {
            var serializedGraphView = new SerializedGraphView();
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
            var guids = new Dictionary<string, GraphElement>();
            foreach (var serializedGraphElement in serializedGraphElements)
            {
                var graphElement = serializedGraphElement.Deserialize(null, m_GraphView);
                graphElement.Query<GraphElement>().ForEach(e => guids[e.viewDataKey] = e);
            }
            Handler.Update();
            foreach (var serializedGraphElement in serializedGraphElements)
            {
                PostDeserialize(serializedGraphElement, guids);
                if (guids.TryGetValue(serializedGraphElement.Guid, out var graphElement))
                {
                    var graphElementObject = CreateGraphElementObject(graphElement);
                    Handler.AddGraphElementObject(graphElementObject);
                }
            }
            Handler.ApplyModifiedProperties();
        }

        void FullReload()
        {
            if (m_GraphViewObjectHandler == null)
                return;
            if (!Handler.CanFullReload)
                return;
            var serializedGraphElements = Handler.GraphViewObject.SerializedGraphElements.ToDictionary(element => element.Guid, element => element);
            var elementsToRemove = new HashSet<GraphElement>(m_GraphView.graphElements.ToList());
            elementsToRemove.RemoveWhere(element => serializedGraphElements.ContainsKey(element.viewDataKey));
            m_GraphView.DeleteElements(elementsToRemove);
            var guids = m_GraphView.graphElements.ToList().ToDictionary(element => element.viewDataKey, element => element);
            foreach (var serializedGraphElement in serializedGraphElements)
            {
                guids.TryGetValue(serializedGraphElement.Key, out var graphElement);
                graphElement = serializedGraphElement.Value.Deserialize(graphElement, m_GraphView);
                graphElement.Query<GraphElement>().ForEach(e => guids[e.viewDataKey] = e);
            }
            foreach (var serializedGraphElement in serializedGraphElements)
            {
                PostDeserialize(serializedGraphElement.Value, guids);
            }
        }

        void OnPortChanged(Edge edge)
        {
            if (edge.isGhostEdge)
                return;
            if (Handler.GraphViewObject.GuidToIndices.TryGetValue(edge.viewDataKey, out var index))
            {
                if (edge.input != null && edge.output != null)
                {
                    Handler.Update();
                    edge.Serialize(Handler.GetGraphElementObjectAtIndex(index));
                }
            }
            m_GraphView.OnValueChanged(edge);
        }

        void AddToken(Edge edge, DropdownMenuAction action)
        {
            var inputPort = Port.Create<TEdge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, edge.output.portType);
            var outputPort = Port.Create<TEdge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, edge.input.portType);
            var token = new TokenNode<TEdge>(inputPort, outputPort);
            AddElement(token, m_EditorWindow.position.position + action.eventInfo.mousePosition);
            edge.input.Disconnect(edge);
            inputPort.Connect(edge);
            edge.input.ConnectTo<TEdge>(outputPort);
            edge.input = inputPort;
        }

        public void PostDeserialize(ISerializedGraphElement serializedGraphElement, Dictionary<string, GraphElement> guids)
        {
            var graphElement = guids[serializedGraphElement.Guid];
            if (graphElement is Edge edge)
            {
                guids.TryGetValue(serializedGraphElement.ReferenceGuids[0], out var inputPort);
                guids.TryGetValue(serializedGraphElement.ReferenceGuids[1], out var outputPort);
                var changed = false;
                if (edge.input != inputPort && inputPort is Port ip)
                {
                    edge.input?.Disconnect(edge);
                    edge.input = ip;
                    edge.input.Connect(edge);
                    changed = true;
                }
                if (edge.output != outputPort && outputPort is Port op)
                {
                    edge.output?.Disconnect(edge);
                    edge.output = op;
                    edge.output.Connect(edge);
                    changed = true;
                }
                if (edge.output == null || edge.input == null)
                {
                    guids.Remove(edge.viewDataKey);
                    m_GraphView.RemoveElement(edge);
                    changed = true;
                }
                if (changed)
                    m_GraphView.OnValueChanged(edge);
            }
        }

        public void AddElement(GraphElement graphElement, Vector2 screenMousePosition)
        {
            if (m_GraphView.Contains(graphElement))
                throw new UnityException($"{m_GraphView} has already contained {graphElement}.");
            m_GraphView.AddElement(graphElement);
            var position = Rect.zero;
            var root = m_EditorWindow.rootVisualElement;
            position.center = m_GraphView.contentViewContainer.WorldToLocal(root.ChangeCoordinatesTo(root.parent ?? root, screenMousePosition - m_EditorWindow.position.position));
            graphElement.SetPosition(position);
            var graphElementObject = CreateGraphElementObject(graphElement);
            Handler.Update();
            Handler.AddGraphElementObject(graphElementObject);
            Handler.ApplyModifiedProperties();
        }

        GraphElementObject CreateGraphElementObject(GraphElement graphElement, bool withoutUndo = false)
        {
            var graphElementObject = ScriptableObject.CreateInstance<GraphElementObject>();
            graphElement.Serialize(graphElementObject);
            if (!withoutUndo)
                Undo.RegisterCreatedObjectUndo(graphElementObject, $"Create {graphElement.GetType().Name}");
            return graphElementObject;
        }

        void OnSelectedElementsChanged(List<ISelectable> selection)
        {
            var ids = new List<int>(selection.Count);
            foreach (var selectable in selection)
            {
                if (selectable is GraphElement element)
                {
                    var index = Handler.GraphViewObject.GuidToIndices[element.viewDataKey];
                    Handler.Update();
                    var graphElementObject = Handler.GetGraphElementObjectAtIndex(index);
                    ids.Add(graphElementObject.GetInstanceID());
                }
            }
            Selection.instanceIDs = ids.ToArray();
        }

        void OnValueChanged(string guid)
        {
            var element = m_GraphView.GetElementByGuid(guid);
            if (Handler.GraphViewObject.GuidToIndices.TryGetValue(guid, out var index))
            {
                if (element is IFieldHolder fieldHolder)
                    fieldHolder.OnValueChanged();
                Handler.Update();
                element.Serialize(Handler.GetGraphElementObjectAtIndex(index));
            }
            m_GraphView.OnValueChanged(element);
        }
    }
}
