using System.Collections;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    sealed class AssetGroup : IEnumerable<UnityObject>
    {
        readonly List<UnityObject> m_Assets = new List<UnityObject>();

        public AssetGroup(IEnumerable<UnityObject> assets)
        {
            m_Assets.AddRange(assets);
        }

        public AssetGroup() { }

        IEnumerator<UnityObject> IEnumerable<UnityObject>.GetEnumerator() => m_Assets.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => m_Assets.GetEnumerator();

        public void Add(AssetGroup x)
        {
            m_Assets.AddRange(x.m_Assets);
        }
    }
}
