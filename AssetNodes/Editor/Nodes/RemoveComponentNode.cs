using System;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Modify/Remove Component")]
    sealed class RemoveComponentNode : INodeProcessor
    {
        sealed class Modifier : IPrefabModifier
        {
            readonly RemoveComponentNode node;
            readonly Type componentType;

            public bool IncludeChildren => node.m_IncludeChildren;
            public string RegexPattern => node.m_RegexPattern;

            public Modifier(RemoveComponentNode node, Type componentType)
            {
                this.node = node;
                this.componentType = componentType;
            }

            public void Modify(GameObject go)
            {
                if (go.TryGetComponent(componentType, out var comp))
                    UnityEngine.Object.DestroyImmediate(comp, true);
            }
        }

        RemoveComponentNode() { }

        [SerializeField]
        bool m_IncludeChildren = false;
        [SerializeField]
        string m_RegexPattern = string.Empty;
        [ComponentPath]
        [SerializeField]
        string m_MenuPath = "";

        public Color HeaderColor => ColorDefinition.ModifyNode;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            if (UnityObjectTypeUtility.TryGetComponentTypeFromMenuPath(m_MenuPath, out var componentType))
                new Modifier(this, componentType).ModifyPrefab(assetGroup);
            container.SetOutput(0, assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
