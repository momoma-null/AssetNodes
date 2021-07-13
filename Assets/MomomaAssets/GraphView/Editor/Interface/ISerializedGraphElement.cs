using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;

    public interface ISerializedGraphElement
    {
        string Guid { get; set; }
        Rect Position { get; set; }
        IReadOnlyList<string> ReferenceGuids { get; set; }
        IGraphElementData? GraphElementData { get; set; }
    }

    public static class SerializedGraphElementExtensions
    {
        public static void Serialize<T>(this GraphElement graphElement, T serializedGraphElement, Rect position) where T : ISerializedGraphElement
        {
            if (graphElement is IFieldHolder fieldHolder)
            {
                serializedGraphElement.GraphElementData = fieldHolder.GraphElementData;
                if (serializedGraphElement is ScriptableObject scriptableObject)
                {
                    var so = new SerializedObject(scriptableObject);
                    fieldHolder.Bind(so);
                }
            }
            serializedGraphElement.Guid = graphElement.viewDataKey;
            serializedGraphElement.Position = position;
            serializedGraphElement.RebindReferenceGuids();
        }

        public static GraphElement Deserialize(this ISerializedGraphElement serializedGraphElement, GraphView graphView)
        {
            if (serializedGraphElement.GraphElementData == null)
                throw new ArgumentNullException(nameof(serializedGraphElement.GraphElementData));
            var graphElement = serializedGraphElement.GraphElementData.Deserialize();
            graphView.AddElement(graphElement);
            if (serializedGraphElement is ScriptableObject scriptableObject && graphElement is IFieldHolder fieldHolder)
            {
                var so = new SerializedObject(scriptableObject);
                fieldHolder.Bind(so);
            }
            serializedGraphElement.Deserialize(graphElement, graphView);
            return graphElement;
        }

        public static void Deserialize(this ISerializedGraphElement serializedGraphElement, GraphElement graphElement, GraphView graphView)
        {
            graphElement.viewDataKey = serializedGraphElement.Guid;
            graphElement.SetPosition(serializedGraphElement.Position);
            serializedGraphElement.GraphElementData?.DeserializeOverwrite(graphElement, graphView);
            serializedGraphElement.RebindReferenceGuids();
        }

        static void RebindReferenceGuids(this ISerializedGraphElement serializedGraphElement)
        {
            var referenceGuids = new List<string>();
            switch (serializedGraphElement.GraphElementData)
            {
                case INodeData nodeData:
                    referenceGuids.AddRange(nodeData.Processor.InputPorts.Select(i => i.Id));
                    referenceGuids.AddRange(nodeData.Processor.OutputPorts.Select(i => i.Id));
                    break;
                case IEdgeData edgeData:
                    referenceGuids.Add(edgeData.InputPortGuid);
                    referenceGuids.Add(edgeData.OutputPortGuid);
                    break;
            }
            serializedGraphElement.ReferenceGuids = referenceGuids;
        }
    }
}
