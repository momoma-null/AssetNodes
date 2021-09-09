using System;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

//#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class Port<T> : Port where T : Edge, new()
    {
        public Port(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type, IEdgeConnectorListener connectorListener)
         : base(portOrientation, portDirection, portCapacity, type)
        {
            m_EdgeConnector = new EdgeConnector<T>(connectorListener);
            this.AddManipulator(m_EdgeConnector);
        }
    }
}
