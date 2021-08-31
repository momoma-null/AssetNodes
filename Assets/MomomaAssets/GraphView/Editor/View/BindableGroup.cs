using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class BindableGroup : Group, IBindableGraphElement
    {
        readonly IGroupData m_GroupData;

        public IGraphElementData GraphElementData => m_GroupData;

        public BindableGroup(IGroupData groupData) : base()
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
                titleField.isDelayed = true;
                var nameProperty = serializedObject.FindProperty("m_GraphElementData.m_Name");
                titleField.BindProperty(nameProperty);
            }
        }

        public void SetPositionWhenDeserialization(Rect position)
        {
            SetScopePositionOnly(position);
        }
    }
}
