using System;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface ISelectableCallback
    {
        event Action<GraphElement> onSelected;
    }
}
