using System;
using System.Reflection;
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
        string TypeName { get; set; }
        Rect Position { get; set; }
        IList<string> ReferenceGuids { get; }
        IGraphElementData? GraphElementData { get; set; }
    }

    public static class SerializedGraphElementExtensions
    {
        static readonly Dictionary<string, ConstructorInfo> s_ConstructorInfos = new Dictionary<string, ConstructorInfo>();

        public static void Serialize<T, TGraphView>(this GraphElement graphElement, T serializedGraphElement, TGraphView graphView) where T : ISerializedGraphElement where TGraphView : GraphView, IGraphViewCallback
        {
            serializedGraphElement.Guid = graphElement.viewDataKey;
            serializedGraphElement.TypeName = graphElement.GetType().AssemblyQualifiedName;
            serializedGraphElement.Position = graphElement.GetPosition();
            var referenceGuids = serializedGraphElement.ReferenceGuids;
            switch (graphElement)
            {
                case Node node:
                    node.Query<Port>().ForEach(port => referenceGuids.Add(port.viewDataKey));
                    break;
                case Edge edge:
                    referenceGuids.Add(edge.input.viewDataKey);
                    referenceGuids.Add(edge.output.viewDataKey);
                    break;
            }
            if (graphElement is IFieldHolder fieldHolder)
            {
                serializedGraphElement.GraphElementData = fieldHolder.GraphElementData;
                if (serializedGraphElement is ScriptableObject scriptableObject)
                    fieldHolder.Bind(new SerializedObject(scriptableObject));
            }
        }

        public static GraphElement Deserialize<TGraphView>(this ISerializedGraphElement serializedGraphElement, GraphElement? graphElement, TGraphView graphView) where TGraphView : GraphView, IGraphViewCallback
        {
            if (graphElement == null)
            {
                graphElement = serializedGraphElement.GraphElementData switch
                {
                    INodeData nodeData => new NodeGUI(nodeData),
                    IEdgeData edgeData => new AdvancedEdge(),
                    _ => throw new ArgumentOutOfRangeException(nameof(serializedGraphElement.GraphElementData))
                };
                graphView.AddElement(graphElement);
                if (serializedGraphElement is ScriptableObject scriptableObject && graphElement is IFieldHolder fieldHolder)
                {
                    var so = new SerializedObject(scriptableObject);
                    fieldHolder.Bind(so);
                }
            }
            graphElement.viewDataKey = serializedGraphElement.Guid;
            graphElement.SetPosition(serializedGraphElement.Position);
            return graphElement;
        }
    }
}
