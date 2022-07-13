using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Object;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Group/Filter by Path")]
    sealed class FilterByPathNode : INodeProcessor
    {
        FilterByPathNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<UnityObject>(isMulti: true);
            portDataContainer.AddInputPort<string>("Path");
            portDataContainer.AddOutputPort<UnityObject>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            var pathData = container.Get(portDataContainer.InputPorts[1], PathData.combine);
            //assetGroup.RemoveWhere(asset => pathData.GetPath)
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
