using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class NodeGUI : Node, IFieldHolder, INotifyValueChanged<bool>, IBindable
    {
        readonly INodeData m_Node;

        public IGraphElementData GraphElementData => m_Node;
        public bool value
        {
            get => expanded;
            set
            {
                if (value == expanded)
                    return;
                if (panel != null)
                {
                    using (var evt = ChangeEvent<bool>.GetPooled(expanded, value))
                    {
                        evt.target = this;
                        SetValueWithoutNotify(value);
                        SendEvent(evt);
                    }
                }
                else
                {
                    SetValueWithoutNotify(value);
                }
            }
        }
        public IBinding? binding { get; set; }
        public string? bindingPath { get; set; }

        public NodeGUI(INodeData nodeData) : base()
        {
            m_Node = nodeData;
            style.minWidth = 200f;
            style.maxWidth = 400f;
            extensionContainer.style.backgroundColor = new Color(0.1803922f, 0.1803922f, 0.1803922f, 0.8039216f);
            var nodeTypeName = m_Node.Processor.GetType().Name;
            title = Regex.Replace(nodeTypeName, "([a-z])([A-Z])", "$1 $2").Replace("Node", "");
            m_CollapseButton.schedule.Execute(FixCollapseButtonEnable).Every(0);
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            expanded = newValue;
            MarkDirtyRepaint();
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
            value = !expanded;
        }

        public void Bind(SerializedObject serializedObject)
        {
            extensionContainer.Clear();
            this.BindProperty(serializedObject.FindProperty("m_GraphElementData.m_Expanded"));
            if (m_Node.GraphElementEditor.UseDefaultVisualElement)
            {
                using (var iterator = serializedObject.FindProperty("m_GraphElementData.m_Processor"))
                using (var endProperty = iterator.GetEndProperty(false))
                {
                    if (iterator.NextVisible(true))
                    {
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
            }
            else
            {
                var field = new IMGUIContainer(() => OnGUIHandler(serializedObject)) { cullingEnabled = true };
                extensionContainer.Add(field);
            }
            RefreshExpandedState();
        }

        void OnGUIHandler(SerializedObject serializedObject)
        {
            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.fieldWidth = 93f;
            EditorGUIUtility.labelWidth = 93f;
            serializedObject.UpdateIfRequiredOrScript();
            if (serializedObject.targetObject is GraphElementObject graphElementObject)
            {
                using (var prop = serializedObject.FindProperty("m_GraphElementData"))
                    graphElementObject.GraphElementData?.GraphElementEditor?.OnGUI(prop);
            }
            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }
    }
}
