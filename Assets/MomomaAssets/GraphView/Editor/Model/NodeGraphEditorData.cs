using System;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    [Serializable]
    public sealed class NodeGraphEditorData
    {
        [SerializeReference]
        internal IGraphElementEditor[] m_Editors = Array.Empty<IGraphElementEditor>();
        [SerializeField]
        internal GraphViewObject? m_SelectedGraphViewObject;
        [SerializeField]
        internal string m_ViewDataKey = Guid.NewGuid().ToString();

        public void OnDisable(bool isDestroying)
        {
            foreach (var e in m_Editors)
                e.OnDisable(isDestroying);
        }
    }
}
