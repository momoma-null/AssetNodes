using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class GraphViewObjectHandler : IDisposable, IEquatable<GraphViewObjectHandler>
    {
        public GraphViewObjectHandler(GraphViewObject graphViewObject, Type graphViewType, Action<string> onGraphElementChanged, Action onGraphViewChanged, NodeGraphProcessor nodeGraphProcessor)
        {
            m_GraphViewObject = graphViewObject;
            m_SerializedGraphView = graphViewObject;
            m_SerializedObject = new SerializedObject(m_GraphViewObject);
            m_SerializedGraphElementsProperty = m_SerializedObject.FindProperty("m_SerializedGraphElements");
            using (var sp = m_SerializedObject.FindProperty("m_GraphViewTypeName"))
                sp.stringValue = graphViewType.AssemblyQualifiedName;
            m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
            m_OnGraphElementChanged = onGraphElementChanged;
            foreach (var element in m_SerializedGraphView.SerializedGraphElements)
            {
                if (element is GraphElementObject graphElementObject)
                {
                    graphElementObject.onValueChanged += m_OnGraphElementChanged;
                }
            }
            m_OnGraphViewChanged = onGraphViewChanged;
            m_NodeGraphProcessor = nodeGraphProcessor;
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        ~GraphViewObjectHandler() => Dispose();

        readonly Action<string> m_OnGraphElementChanged;
        readonly Action m_OnGraphViewChanged;
        readonly NodeGraphProcessor m_NodeGraphProcessor;
        readonly SerializedObject m_SerializedObject;
        readonly SerializedProperty m_SerializedGraphElementsProperty;
        readonly GraphViewObject m_GraphViewObject;
        readonly ISerializedGraphView m_SerializedGraphView;

        public bool IsValid => m_GraphViewObject != null;
        public IReadOnlyDictionary<string, ISerializedGraphElement> GuidToSerializedGraphElements => m_SerializedGraphView.GuidtoSerializedGraphElements;
        public Action StartProcess => () => { if (m_GraphViewObject != null) m_NodeGraphProcessor.StartProcess(m_GraphViewObject); };

        public void Dispose()
        {
            if (m_GraphViewObject != null)
            {
                foreach (var element in m_SerializedGraphView.SerializedGraphElements)
                {
                    if (element is GraphElementObject graphElementObject)
                    {
                        graphElementObject.onValueChanged -= m_OnGraphElementChanged;
                    }
                }
            }
            Undo.undoRedoPerformed -= UndoRedoPerformed;
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

        public GraphElementObject? TryGetGraphElementObjectByGuid(string guid)
        {
            if (m_SerializedGraphView.GuidtoSerializedGraphElements.TryGetValue(guid, out var serializedGraphElement))
                return serializedGraphElement as GraphElementObject;
            return null;
        }

        void UndoRedoPerformed()
        {
            m_OnGraphViewChanged();
            RegisterAllValueChangedEvents();
        }

        void RegisterAllValueChangedEvents()
        {
            foreach (var element in m_SerializedGraphView.SerializedGraphElements)
            {
                if (element is GraphElementObject graphElementObject)
                {
                    graphElementObject.onValueChanged -= m_OnGraphElementChanged;
                    graphElementObject.onValueChanged += m_OnGraphElementChanged;
                }
            }
        }

        public sealed class SetScope : IDisposable
        {
            readonly GraphViewObjectHandler m_Handler;
            readonly bool m_WithoutUndo;
            readonly HashSet<GraphElementObject> m_ToDeleteAssets = new HashSet<GraphElementObject>();
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
                    {
                        if (i.GraphElementData is IAdditionalAssetHolder assetHolder)
                        {
                            foreach (var j in assetHolder.Assets)
                                DestroyImmediate(j, true);
                        }
                        DestroyImmediate(i, true);
                    }
                }
                else
                {
                    foreach (var i in m_ToDeleteAssets)
                    {
                        if (i.GraphElementData is IAdditionalAssetHolder assetHolder)
                        {
                            foreach (var j in assetHolder.Assets)
                                Undo.DestroyObjectImmediate(j);
                        }
                        Undo.DestroyObjectImmediate(i);
                    }
                }
                AssetDatabase.SaveAssets();
            }

            public void AddGraphElementObject(GraphElementObject graphElementObject)
            {
                var path = AssetDatabase.GetAssetPath(m_Handler.m_GraphViewObject);
                graphElementObject.hideFlags &= ~HideFlags.DontSaveInEditor;
                AssetDatabase.AddObjectToAsset(graphElementObject, path);
                if (!m_WithoutUndo)
                    Undo.RegisterCreatedObjectUndo(graphElementObject, $"Create {nameof(GraphElementObject)}");
                if (graphElementObject.GraphElementData is IAdditionalAssetHolder assetHolder)
                {
                    foreach (var i in assetHolder.Assets)
                    {
                        if (AssetDatabase.Contains(i))
                            continue;
                        AssetDatabase.AddObjectToAsset(i, path);
                        if (!m_WithoutUndo)
                            Undo.RegisterCreatedObjectUndo(i, $"Create {i.name}");
                    }
                }
                graphElementObject.onValueChanged += m_Handler.m_OnGraphElementChanged;
                ++m_Handler.m_SerializedGraphElementsProperty.arraySize;
                using (var sp = m_Handler.m_SerializedGraphElementsProperty.GetArrayElementAtIndex(m_Handler.m_SerializedGraphElementsProperty.arraySize - 1))
                    sp.objectReferenceValue = graphElementObject;
                m_ToDeleteAssets.Remove(graphElementObject);
            }

            public void DeleteGraphElementObjects(HashSet<string> guids)
            {
                var indices = new SortedSet<int>();
                var i = 0;
                foreach (var element in m_Handler.m_SerializedGraphView.SerializedGraphElements)
                {
                    if (element != null && guids.Contains(element.Guid))
                        indices.Add(i);
                    ++i;
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
}
