using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.AssetProcessor
{
    [InitializeOnLoad]
    sealed class LoadAssetsNode : INode
    {
        static LoadAssetsNode()
        {
            NodeUtility.AddConstructor(() => new LoadAssetsNode());
        }

        public string Title => "Load Assets";
        public IEnumerable<PortData> InputPorts => Array.Empty<PortData>();
        public IEnumerable<PortData> OutputPorts => new[] { new PortData(typeof(UnityObject)) };

        DefaultAsset? m_Folder;

        public IEnumerable<PropertyValue> GetProperties()
        {
            yield return PropertyValue.Create(UnityObjectWrapper.Create(m_Folder));
        }
    }
}
