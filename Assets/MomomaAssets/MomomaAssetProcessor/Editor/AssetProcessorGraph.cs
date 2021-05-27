using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.AssetProcessor
{
    sealed class AssetProcessorGraph : GraphView
    {
        public AssetProcessorGraph() { }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var linkedPorts = new HashSet<Port>();
            CollectLinkedPorts(startPort, linkedPorts);
            return ports.ToList().FindAll(p => p.direction != startPort.direction && !linkedPorts.Contains(p));
        }

        static void CollectLinkedPorts(Port startPort, HashSet<Port> linkedPorts)
        {
            var ports = startPort.direction == Direction.Input ? startPort.node.outputContainer.Query<Port>() : startPort.node.inputContainer.Query<Port>();
            ports.ForEach(p =>
            {
                foreach (var e in p.connections)
                {
                    var pair = startPort.direction == Direction.Input ? e.input : e.output;
                    CollectLinkedPorts(pair, linkedPorts);
                }
            });
        }
    }
}
