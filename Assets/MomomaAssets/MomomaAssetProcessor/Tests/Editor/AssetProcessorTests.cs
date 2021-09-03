using NUnit.Framework;
using MomomaAssets.GraphView;
using MomomaAssets.GraphView.AssetProcessor;

namespace MomomaAssets.GraphView.AssetProcessor.Tests
{
    sealed class AssetProcessorTests
    {
        [Test]
        public void ProcessAll()
        {
            foreach (var i in GraphViewObject.GetGraphViewObjects<AssetProcessorGUI>())
                CoreAssetProcessor.s_NodeGraphProcessor.StartProcess(i);
        }
    }
}
