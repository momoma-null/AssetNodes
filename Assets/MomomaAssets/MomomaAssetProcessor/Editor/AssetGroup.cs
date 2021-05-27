using System.Collections.Generic;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.AssetProcessor
{
    sealed class AssetGroup
    {
        readonly List<UnityObject> m_Assets = new List<UnityObject>();

        public AssetGroup(IEnumerable<UnityObject> assets)
        {
            m_Assets.AddRange(assets);
        }

        public AssetGroup() { }

        public void Add(AssetGroup x)
        {
            m_Assets.AddRange(x.m_Assets);
        }
    }
}
