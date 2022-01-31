using NUnit.Framework;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor.Tests
{
    sealed class AssetProcessorTests
    {
        [SetUp]
        public void SetUp()
        {
            CoreAssetProcessor.IsTesting = true;
        }

        [Test]
        public void ProcessAll()
        {
            foreach (var i in GraphViewObject.GetGraphViewObjects<AssetProcessorGUI>())
                CoreAssetProcessor.s_NodeGraphProcessor.StartProcess(i);
        }

        [TearDown]
        public void TearDown()
        {
            CoreAssetProcessor.IsTesting = false;
        }
    }
}
