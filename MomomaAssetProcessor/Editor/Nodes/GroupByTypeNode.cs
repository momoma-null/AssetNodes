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

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroupPortDefinition.Default);
            var textures = new AssetGroup();
            var materials = new AssetGroup();
            var gameObjects = new AssetGroup();
            var animations = new AssetGroup();
            var meshes = new AssetGroup();
            var scenes = new AssetGroup();
            assetGroup.RemoveWhere(assets =>
            {
                if (assets.MainAssetType == typeof(Texture))
                    textures.Add(assets);
                else if (assets.MainAssetType == typeof(Material))
                    materials.Add(assets);
                else if (assets.MainAssetType == typeof(GameObject))
                    gameObjects.Add(assets);
                else if (assets.MainAssetType == typeof(AnimationClip))
                    animations.Add(assets);
                else if (assets.MainAssetType == typeof(Mesh))
                    meshes.Add(assets);
                else if (assets.MainAssetType == typeof(Scene))
                    scenes.Add(assets);
                else
                    return false;
                return true;
            });
            container.Set(portDataContainer.OutputPorts[0], textures);
            container.Set(portDataContainer.OutputPorts[1], materials);
            container.Set(portDataContainer.OutputPorts[2], gameObjects);
            container.Set(portDataContainer.OutputPorts[3], animations);
            container.Set(portDataContainer.OutputPorts[4], meshes);
            container.Set(portDataContainer.OutputPorts[5], scenes);
            container.Set(portDataContainer.OutputPorts[6], assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
