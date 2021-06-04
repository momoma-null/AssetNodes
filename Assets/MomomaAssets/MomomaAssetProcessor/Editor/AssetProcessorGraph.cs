using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    using GraphView = UnityEditor.Experimental.GraphView.GraphView;

    sealed class AssetProcessorGraph : GraphView, IGraphViewCallback
    {
        public AssetProcessorGraph() { }

        public event Action<List<ISelectable>>? onSelectionChanged;

        public void Initialize() { }

        public void OnValueChanged(VisualElement visualElement) { }

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

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            onSelectionChanged?.Invoke(selection);
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            onSelectionChanged?.Invoke(selection);
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            onSelectionChanged?.Invoke(selection);
        }
    }
}
