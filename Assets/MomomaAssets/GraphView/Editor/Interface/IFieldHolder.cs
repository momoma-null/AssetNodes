using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IFieldHolder
    {
        event Action<GraphElement> onValueChanged;
        IGraphElementData GraphElementData { get; }
        void Bind(SerializedObject serializedObject);
    }
}
