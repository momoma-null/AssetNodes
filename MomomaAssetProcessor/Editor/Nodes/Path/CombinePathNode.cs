using System;
using System.IO;
using System.Linq;

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
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddInputPort(PathDataPortDefinition.Default);
            portDataContainer.AddOutputPort(PathDataPortDefinition.Default);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var pathDatas = portDataContainer.InputPorts.Select(port => container.Get(port, PathData.combine)).ToArray();
            container.Set(portDataContainer.OutputPorts[0],
                new PathData(asset => Path.Combine(pathDatas.Select(path => path.GetPath(asset)).ToArray())));
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
