using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    [Serializable]
    public sealed class NodeGraphEditorData
    {
        [SerializeField]
        internal GraphViewObject? m_SelectedGraphViewObject;
        [SerializeField]
        internal string m_ViewDataKey = Guid.NewGuid().ToString();
    }
}
