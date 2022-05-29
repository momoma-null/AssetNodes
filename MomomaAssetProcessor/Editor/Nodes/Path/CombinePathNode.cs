using System;
using System.IO;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Path/Combine Paths")]
    sealed class CombinePathNode : INodeProcessor
    {
        CombinePathNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<string>();
            portDataContainer.AddInputPort<string>();
            portDataContainer.AddOutputPort<string>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var pathData0 = container.Get(portDataContainer.InputPorts[0], PathData.combine);
            var pathData1 = container.Get(portDataContainer.InputPorts[1], PathData.combine);
            container.Set(portDataContainer.OutputPorts[0],
                new PathData(asset => Path.Combine(pathData0.GetPath(asset), pathData1.GetPath(asset))));
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
