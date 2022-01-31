using System;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class NodeGraphProcessor
    {
        readonly Action? PreProcess;
        readonly Action? PostProcess;
        readonly Action? Completed;

        public NodeGraphProcessor(Action? preProcess = null, Action? postProcess = null, Action? completed = null)
        {
            PreProcess = preProcess;
            PostProcess = postProcess;
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
            try
            {
                PreProcess?.Invoke();
                nodeData.Processor.Process(container, nodeData);
            }
            finally
            {
                PostProcess?.Invoke();
            }
        }
    }
}
