using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView
{
    public static class UnityObjectExtensions
    {
        public static void DestroyImmediate(this Object obj)
        {
            Debug.Log(obj);
            if (obj != null)
                Object.DestroyImmediate(obj);
        }
    }
}
