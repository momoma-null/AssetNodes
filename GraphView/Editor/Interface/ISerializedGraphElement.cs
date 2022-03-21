using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

//#nullable enable

namespace MomomaAssets.GraphView
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;

    interface ISerializedGraphElement
    {
        string Guid { get; set; }
        Rect Position { get; set; }
        IGraphElementData GraphElementData { get; set; }
        SerializedObject SerializedObject { get; }
    }

    static class SerializedGraphElementExtensions
    {
        public static void Serialize<T>(this GraphElement graphElement, T serializedGraphElement, Rect position) where T : ISerializedGraphElement
        {
            if (graphElement is IBindableGraphElement bindableGraphElement)
            {
                serializedGraphElement.GraphElementData = bindableGraphElement.GraphElementData;
                if (serializedGraphElement.SerializedObject != null)
                {
                    bindableGraphElement.Bind(serializedGraphElement.SerializedObject);
                }
            }
            serializedGraphElement.Guid = graphElement.viewDataKey;
            serializedGraphElement.Position = position;
        }

        public static GraphElement Deserialize(this ISerializedGraphElement serializedGraphElement, GraphView graphView)
        {
            if (serializedGraphElement.GraphElementData == null)
                throw new ArgumentNullException(nameof(serializedGraphElement.GraphElementData));
            var graphElement = serializedGraphElement.GraphElementData.Deserialize();
            graphView.AddElement(graphElement);
            if (serializedGraphElement.SerializedObject != null && graphElement is IBindableGraphElement bindableGraphElement)
            {
                bindableGraphElement.Bind(serializedGraphElement.SerializedObject);
            }
            serializedGraphElement.Deserialize(graphElement, graphView);
            return graphElement;
        }

        public static void Deserialize(this ISerializedGraphElement serializedGraphElement, GraphElement graphElement, GraphView graphView)
        {
            graphElement.viewDataKey = serializedGraphElement.Guid;
            serializedGraphElement.GraphElementData?.SetPosition(graphElement, serializedGraphElement.Position);
            serializedGraphElement.GraphElementData?.DeserializeOverwrite(graphElement, graphView);
        }
    }
}
