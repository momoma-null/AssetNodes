using System;

//#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class NodeGraphProcessor
    {
        readonly Action Completed;

        public NodeGraphProcessor(Action completed = null)
        {
            Completed = completed;
        }

        public void StartProcess(GraphViewObject graphViewObject)
        {
            var guidToSerializedGraphElements = (graphViewObject as ISerializedGraphView).GuidtoSerializedGraphElements;
            var container = new ProcessingDataContainer(GetData, guidToSerializedGraphElements);
            foreach (var i in container.EndNodeDatas)
            {
                GetData(i, container);
            }
            container.Clear();
            Completed?.Invoke();
        }

        void GetData(INodeData nodeData, ProcessingDataContainer container)
        {
            var containerParts = new ProcessingDataContainer.Parts(container, nodeData);
            nodeData.Processor.Process(containerParts);
        }
    }
}
