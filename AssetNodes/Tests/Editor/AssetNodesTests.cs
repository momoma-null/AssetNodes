using NUnit.Framework;

//#nullable enable

namespace MomomaAssets.GraphView.AssetNodes.Tests
{
    sealed class AssetNodesTests
    {
        [SetUp]
        public void SetUp()
        {
            CoreAssetNodes.IsTesting = true;
        }

        [Test]
        public void ProcessAll()
        {
            foreach (var i in GraphViewObject.GetGraphViewObjects<AssetNodesGUI>())
                CoreAssetNodes.s_NodeGraphProcessor.StartProcess(i);
        }

        [TearDown]
        public void TearDown()
        {
            CoreAssetNodes.IsTesting = false;
        }
    }
}
