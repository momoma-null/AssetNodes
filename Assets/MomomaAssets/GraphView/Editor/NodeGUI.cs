using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class NodeGUI : Node, IFieldHolder
    {
        readonly INodeData m_Node;
        SerializedObject? m_SerializedObject;

        public IGraphElementData GraphElementData => m_Node;

        public NodeGUI(INodeData nodeData) : base()
        {
            m_Node = nodeData;
            style.minWidth = 150f;
            extensionContainer.style.backgroundColor = new Color(0.1803922f, 0.1803922f, 0.1803922f, 0.8039216f);
            title = m_Node.Title;
            capabilities |= Capabilities.Renamable;
            m_CollapseButton.schedule.Execute(() =>
            {
                if (!m_CollapseButton.enabledInHierarchy)
                {
                    m_CollapseButton.SetEnabled(false);
                    m_CollapseButton.SetEnabled(true);
                }
            }).Every(0);
        }

        public void Bind(SerializedObject serializedObject)
        {
            m_SerializedObject = serializedObject;
            extensionContainer.Clear();
            var dataProperty = serializedObject.FindProperty("m_GraphElementData");
            var field = new PropertyField(dataProperty);
            extensionContainer.Add(field);
            field.BindProperty(dataProperty);
            RefreshExpandedState();
        }

        public void Update()
        {
            m_SerializedObject?.UpdateIfRequiredOrScript();
        }
    }
}
