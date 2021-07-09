using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    sealed class GroupByPrefabTypeNode : INodeData
    {
        static GroupByPrefabTypeNode()
        {
            INodeDataUtility.AddConstructor(() => new GroupByPrefabTypeNode());
        }

        GroupByPrefabTypeNode() { }

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();
        public string MenuPath => "Group/Group by PrefabType";
        public IEnumerable<PortData> InputPorts => new[] { m_InputPort };
        public IEnumerable<PortData> OutputPorts => m_OutputPorts;

        [SerializeField]
        [HideInInspector]
        PortData m_InputPort = new PortData(typeof(GameObject));

        [SerializeField]
        [HideInInspector]
        PortData[] m_OutputPorts = new PortData[] { new PortData(typeof(GameObject), nameof(PrefabAssetType.Regular)),
                                                    new PortData(typeof(GameObject), nameof(PrefabAssetType.Model)),
                                                    new PortData(typeof(GameObject), nameof(PrefabAssetType.Variant)) };

        public void Process(ProcessingDataContainer container)
        {
            var assetGroup = container.Get(m_InputPort.Id, () => new AssetGroup());
            var regulars = new AssetGroup();
            var models = new AssetGroup();
            var variants = new AssetGroup();
            foreach (var assets in assetGroup)
            {
                switch (PrefabUtility.GetPrefabAssetType(assets.MainAsset))
                {
                    case PrefabAssetType.Regular: regulars.Add(assets); break;
                    case PrefabAssetType.Model: models.Add(assets); break;
                    case PrefabAssetType.Variant: variants.Add(assets); break;
                }
            }
            container.Set(m_OutputPorts[0].Id, regulars);
            container.Set(m_OutputPorts[1].Id, models);
            container.Set(m_OutputPorts[2].Id, variants);
        }
    }
}
