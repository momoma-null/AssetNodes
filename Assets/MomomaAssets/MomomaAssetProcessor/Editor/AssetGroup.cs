using System.Collections.Generic;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    sealed class AssetGroup : HashSet<UnityObject>
    {
        public AssetGroup() { }
        public AssetGroup(IEnumerable<UnityObject> source) : base(source) { }
    }
}
