using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class NodeGUI : Node, IFieldHolder, IDisposable
    {
        readonly INodeData m_Node;
        SerializedObject? m_SerializedObject;
        SerializedProperty? m_ExpandedProperty;
        Editor? m_CachedEditor;

        public IGraphElementData GraphElementData => m_Node;

        public NodeGUI(INodeData nodeData) : base()
        {
            m_Node = nodeData;
            style.minWidth = 200f;
            extensionContainer.style.backgroundColor = new Color(0.1803922f, 0.1803922f, 0.1803922f, 0.8039216f);
            var nodeTypeName = m_Node.Processor.GetType().Name;
            title = Regex.Replace(nodeTypeName, "([a-z])([A-Z])", "$1 $2").Replace("Node", "");
            m_CollapseButton.schedule.Execute(FixCollapseButtonEnable).Every(0);
        }

        ~NodeGUI()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (m_CachedEditor != null)
                DestroyImmediate(m_CachedEditor);
            m_SerializedObject = null;
            m_ExpandedProperty = null;
        }

        void FixCollapseButtonEnable()
        {
            if (!m_CollapseButton.enabledInHierarchy)
            {
                m_CollapseButton.SetEnabled(false);
                m_CollapseButton.SetEnabled(true);
            }
        }

        protected override void ToggleCollapse()
        {
            base.ToggleCollapse();
            if (m_SerializedObject == null || m_ExpandedProperty == null)
                return;
            m_SerializedObject.UpdateIfRequiredOrScript();
            m_ExpandedProperty.boolValue = expanded;
            m_SerializedObject.ApplyModifiedProperties();
        }

        public void Bind(SerializedObject serializedObject)
        {
            m_SerializedObject = serializedObject;
            extensionContainer.Clear();
            m_ExpandedProperty = serializedObject.FindProperty("m_GraphElementData.m_Expanded");
            if (m_Node.GraphElementEditor.UseDefaultVisualElement)
            {
                using (var iterator = serializedObject.FindProperty("m_GraphElementData.m_Processor"))
                using (var endProperty = iterator.GetEndProperty(false))
                {
                    iterator.NextVisible(true);
                    while (true)
                    {
                        if (SerializedProperty.EqualContents(iterator, endProperty))
                            break;
                        var prop = iterator.Copy();
                        var field = new PropertyField(prop);
                        extensionContainer.Add(field);
                        field.BindProperty(prop);
                        if (!iterator.NextVisible(false))
                            break;
                    }
                }
            }
            else
            {
                Editor.CreateCachedEditor(m_SerializedObject.targetObjects, null, ref m_CachedEditor);
                var field = new IMGUIContainer(OnGUIHandler) { cullingEnabled = true };
                extensionContainer.Add(field);
            }
            RefreshExpandedState();
        }

        void OnGUIHandler()
        {
            if (m_CachedEditor == null)
                return;
            var oldWideMode = EditorGUIUtility.wideMode;
            var oldFieldWidth = EditorGUIUtility.fieldWidth;
            try
            {
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.fieldWidth = 150f;
                m_CachedEditor.OnInspectorGUI();
            }
            finally
            {
                EditorGUIUtility.wideMode = oldWideMode;
                EditorGUIUtility.fieldWidth = oldFieldWidth;
            }
        }

        public void Update()
        {
            m_SerializedObject?.UpdateIfRequiredOrScript();
        }
    }
}
