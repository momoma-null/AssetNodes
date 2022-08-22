using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class BindableNode : Node, IBindableGraphElement, INotifyValueChanged<bool>, IBindable
    {
        readonly INodeData m_Node;

        Editor? m_Editor;

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

        public BindableNode(INodeData nodeData) : base()
        {
            m_Node = nodeData;
            style.minWidth = 200f;
            style.maxWidth = 400f;
            var nodeTypeName = m_Node.Processor.GetType().Name;
            title = ObjectNames.NicifyVariableName(nodeTypeName).Replace("Node", "");
            var backgroundColor = nodeData.Processor.HeaderColor;
            backgroundColor.a = 0.8039216f;
            titleContainer.style.backgroundColor = backgroundColor;
            titleContainer.style.unityFontStyleAndWeight = FontStyle.Bold;
            extensionContainer.style.backgroundColor = new Color(0.1803922f, 0.1803922f, 0.1803922f, 0.8039216f);
            m_CollapseButton.schedule.Execute(FixCollapseButtonEnable).Every(0);
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (m_Editor != null)
            {
                UnityEngine.Object.DestroyImmediate(m_Editor);
                m_Editor = null;
            }
            UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
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
            serializedObject.Update();
            this.BindProperty(serializedObject.FindProperty("m_GraphElementData.m_Expanded"));
            var graphElementEditor = GraphElementEditorFactory.CreateEditor(m_Node, serializedObject.FindProperty("m_GraphElementData"));
            if (graphElementEditor.UseDefaultVisualElement)
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
                RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
                var field = new IMGUIContainer(() => OnGUIHandler(serializedObject.targetObjects)) { cullingEnabled = true };
                extensionContainer.Add(field);
            }
            RefreshExpandedState();
        }

        void OnGUIHandler(UnityObject[] targets)
        {
            Editor.CreateCachedEditor(targets, null, ref m_Editor);
            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.fieldWidth = 93f;
            EditorGUIUtility.labelWidth = 93f;
            m_Editor?.OnInspectorGUI();
        }
    }
}
