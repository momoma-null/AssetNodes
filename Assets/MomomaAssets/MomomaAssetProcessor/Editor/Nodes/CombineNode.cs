using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Group/Combine")]
    sealed class CombineNode : INodeProcessor
    {
        static CombineNode()
        {
            INodeDataUtility.AddConstructor(() => new CombineNode());
        }

        CombineNode() { }

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(UnityObject)));
            portDataContainer.OutputPorts.Add(new PortData(typeof(UnityObject)));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var output = new AssetGroup();
            foreach (var i in portDataContainer.InputPorts)
                output.UnionWith(container.Get(i, this.NewAssetGroup));
            container.Set(portDataContainer.OutputPorts[0], output);
        }
    }
}
