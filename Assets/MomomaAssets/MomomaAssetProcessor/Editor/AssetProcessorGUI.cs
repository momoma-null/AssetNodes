using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.AssetProcessor
{
    sealed class AssetProcessorGUI : EditorWindow
    {
        [MenuItem("MomomaTools/Asset Processor", false, 500)]
        static void ShowWindow()
        {
            EditorWindow.GetWindow<AssetProcessorGUI>("MomomaAssetProcessor");
        }

        void OnEnable()
        {
            rootVisualElement.Add(new AssetProcessorGraph(this));
        }
    }
}
