using UnityEngine;

//#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    static class ColorDefinition
    {
        public static Color PathNode { get; } = new Color(0.894f, 0.368f, 0.196f);
        public static Color ModifyNode { get; } = new Color(0.254f, 0.411f, 0.882f);
        public static Color FilterNode { get; } = new Color(0.180f, 0.545f, 0.341f);
        public static Color IONode { get; } = new Color(0.321f, 0.305f, 0.301f);
        public static Color CleanupNode { get; } = new Color(0.086f, 0.368f, 0.513f);
        public static Color ValidateNode { get; } = new Color(0.403f, 0.270f, 0.596f);
        public static Color ImporterNode { get; } = new Color(0.843f, 0.000f, 0.227f);
        public static Color AddressableNode { get; } = new Color(0.784f, 0.600f, 0.196f);
    }
}
