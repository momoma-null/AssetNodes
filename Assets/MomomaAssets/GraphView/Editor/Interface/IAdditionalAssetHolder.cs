using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IAdditionalAssetHolder
    {
        IEnumerable<Object> Assets { get; }
        void OnClone();
    }

    public static class AdditionalAssetHolderExtensions
    {
        public static IEnumerable<Object> CloneAssets(this IAdditionalAssetHolder assetHolder)
        {
            foreach (var i in assetHolder.Assets)
            {
                if (i == null)
                    continue;
                var newAsset = Object.Instantiate(i);
                newAsset.name = i.name;
                yield return newAsset;
            }
        }
    }
}
