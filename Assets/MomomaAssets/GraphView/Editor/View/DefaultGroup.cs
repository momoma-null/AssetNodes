using UnityEditor;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class DefaultGroup : Group, IFieldHolder
    {
        public DefaultGroup(IGroupData groupData) : base()
        {
            m_GroupData = groupData;
            title = groupData.Name;
        }

        readonly IGroupData m_GroupData;

        public IGraphElementData GraphElementData => m_GroupData;

        public void Bind(SerializedObject serializedObject) { }
        public void Update() { }
    }
}
