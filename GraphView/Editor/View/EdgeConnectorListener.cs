using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;

    sealed class EdgeConnectorListener : IEdgeConnectorListener
    {
        public static EdgeConnectorListener Default { get; } = new EdgeConnectorListener();

        GraphViewChange m_GraphViewChange;

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            var graphView = edge.GetFirstAncestorOfType<GraphView>();
            if (graphView != null)
            {
                var context = new NodeCreationContext();
                context.screenMousePosition = GUIUtility.GUIToScreenPoint(position);
                context.target = edge.input ?? edge.output;
                graphView.nodeCreationRequest(context);
            }
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            var edgesToDelete = new List<GraphElement>();
            if (edge.input.capacity == Port.Capacity.Single)
                edgesToDelete.AddRange(edge.input.connections.Where(e => e != edge));
            if (edge.output.capacity == Port.Capacity.Single)
                edgesToDelete.AddRange(edge.output.connections.Where(e => e != edge));
            if (edgesToDelete.Count > 0)
                graphView.DeleteElements(edgesToDelete);

            var edgesToCreate = new List<Edge>() { edge };
            if (graphView.graphViewChanged != null)
            {
                m_GraphViewChange.edgesToCreate = edgesToCreate;
                edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
            }

            foreach (var e in edgesToCreate)
            {
                graphView.AddElement(e);
                edge.input.Connect(e);
                edge.output.Connect(e);
            }
        }
    }
}
