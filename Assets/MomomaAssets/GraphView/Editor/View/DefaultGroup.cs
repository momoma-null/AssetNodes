using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class DefaultGroup : Group, IFieldHolder
    {
        readonly IGroupData m_GroupData;

        public IGraphElementData GraphElementData => m_GroupData;

        public DefaultGroup(IGroupData groupData) : base()
        {
            m_GroupData = groupData;
            title = groupData.Name;
            this.AddManipulator(new ContextualMenuManipulator(null));
        }

        public void Bind(SerializedObject serializedObject)
        {
            var titleField = headerContainer.Q<TextField>();
            if (titleField != null)
            {
                var nameProperty = serializedObject.FindProperty("m_GraphElementData.m_Name");
                titleField.BindProperty(nameProperty);
            }
        }

        public void Update() { }

        public void SetPositionWhenDeserialization(Rect position)
        {
            SetScopePositionOnly(position);
        }
    }
}
