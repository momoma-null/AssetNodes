using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    public sealed class PathData : IProcessingData
    {
        public static Func<IEnumerable<PathData>, PathData> combine = pathDatas =>
        {
            foreach (var i in pathDatas)
                return new PathData(i.getPath);
            return new PathData(asset => string.Empty);
        };

        Func<AssetData, string> getPath;

        public PathData(Func<AssetData, string> getPath)
        {
            this.getPath = getPath;
        }

        public string GetPath(AssetData assetData) => getPath(assetData);
    }

    public sealed class PathDataPortDefinition : IPortDefinition<PathData>
    {
        public static PathDataPortDefinition Default { get; } = new PathDataPortDefinition();

        static readonly Func<AssetData, string> emptyFunc = asset => string.Empty;

        PathDataPortDefinition() { }

        public bool IsMultiInput => false;
        public bool IsMultiOutput => true;
        public Color PortColor { get; } = new Color(0.375f, 0.75f, 0.375f, 1f);

        public PathData CombineInputData(IEnumerable<PathData> inputs)
        {
            foreach (var i in inputs)
                return new PathData(i.GetPath);
            return new PathData(emptyFunc);
        }
    }
}
