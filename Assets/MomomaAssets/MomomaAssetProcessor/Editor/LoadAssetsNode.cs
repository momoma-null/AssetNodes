using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [Serializable]
    sealed class LoadAssetsNode : INodeData
    {
        static LoadAssetsNode()
        {
            INodeDataUtility.AddConstructor(() => new LoadAssetsNode());
        }

        public string Title => "Load Assets";
        public string MenuPath => "Import/Load Assets";
        public IEnumerable<PortData> InputPorts => Array.Empty<PortData>();
        public IEnumerable<PortData> OutputPorts => new[] { new PortData(typeof(UnityObject)) };

        [SerializeField]
        DefaultAsset? m_Folder;

        public IEnumerable<PropertyValue> GetProperties()
        {
            yield return PropertyValue.Create(nameof(m_Folder), UnityObjectWrapper.Create(m_Folder));
        }
    }
}
