using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IGraphViewCallback
    {
        event Action<List<ISelectable>> onSelectionChanged;
        void Initialize();
        void OnValueChanged(VisualElement visualElement);
    }
}
