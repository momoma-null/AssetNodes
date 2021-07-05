using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IAdditionalAssetHolder
    {
        IEnumerable<Object> Assets { get; }
    }
}
