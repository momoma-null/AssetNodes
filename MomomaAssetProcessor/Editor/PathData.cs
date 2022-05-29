using System;
using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
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

        public PathData(Func<AssetData, string> getpath)
        {
            this.getPath = getpath;
        }

        public string GetPath(AssetData assetData) => getPath(assetData);
    }
}
