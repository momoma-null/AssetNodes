using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Group/Group by Type")]
    sealed class GroupByTypeNode : INodeProcessor
    {
        GroupByTypeNode() { }

        public Color HeaderColor => ColorDefinition.FilterNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(Texture));
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(Material));
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(GameObject));
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(AnimationClip));
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(Mesh));
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, nameof(Scene));
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, "Other");
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            var textures = new AssetGroup();
            var materials = new AssetGroup();
            var gameObjects = new AssetGroup();
            var animations = new AssetGroup();
            var meshes = new AssetGroup();
            var scenes = new AssetGroup();
            var others = new AssetGroup();
            foreach(var assets in assetGroup)
            {
                if (typeof(Texture).IsAssignableFrom(assets.MainAssetType))
                    textures.Add(assets);
                else if (typeof(Material).IsAssignableFrom(assets.MainAssetType))
                    materials.Add(assets);
                else if (typeof(GameObject).IsAssignableFrom(assets.MainAssetType))
                    gameObjects.Add(assets);
                else if (typeof(AnimationClip).IsAssignableFrom(assets.MainAssetType))
                    animations.Add(assets);
                else if (typeof(Mesh).IsAssignableFrom(assets.MainAssetType))
                    meshes.Add(assets);
                else if (assets.MainAssetType == typeof(Scene))
                    scenes.Add(assets);
                else
                    others.Add(assets);
            }
            container.SetOutput(0, textures);
            container.SetOutput(1, materials);
            container.SetOutput(2, gameObjects);
            container.SetOutput(3, animations);
            container.SetOutput(4, meshes);
            container.SetOutput(5, scenes);
            container.SetOutput(6, others);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
