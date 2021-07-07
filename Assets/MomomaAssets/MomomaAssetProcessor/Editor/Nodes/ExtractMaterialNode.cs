using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [Serializable]
    sealed class ExtractMaterialNode : INodeData
    {
        static ExtractMaterialNode()
        {
            INodeDataUtility.AddConstructor(() => new ExtractMaterialNode());
        }

        ExtractMaterialNode() { }

        public IGraphElementEditor GraphElementEditor { get; } = new DefaultGraphElementEditor();
        public string MenuPath => "Importer/Extract Material";
        public IEnumerable<PortData> InputPorts => new[] { m_InputPort };
        public IEnumerable<PortData> OutputPorts => new[] { m_OutputPort };

        [SerializeField]
        [HideInInspector]
        PortData m_InputPort = new PortData(typeof(GameObject));

        [SerializeField]
        [HideInInspector]
        PortData m_OutputPort = new PortData(typeof(GameObject));

        [SerializeField]
        string m_DirectoryPath = "../Materials";

        public void Process(ProcessingDataContainer container)
        {
            var assets = container.Get(m_InputPort.Id, () => new AssetGroup());
            foreach (var asset in assets)
            {
                if (asset is GameObject model)
                {
                    var path = AssetDatabase.GetAssetPath(model);
                    if (AssetImporter.GetAtPath(path) is ModelImporter)
                    {
                        var directoryPath = Path.Combine(path, m_DirectoryPath);
                        var isDirty = false;
                        foreach (var i in AssetDatabase.LoadAllAssetsAtPath(path))
                        {
                            if (!(i is Material))
                                continue;
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                                AssetDatabase.ImportAsset(directoryPath);
                            }
                            var dstPath = Path.Combine(directoryPath, $"{i.name}.mat");
                            AssetDatabase.ExtractAsset(i, dstPath);
                            isDirty = true;
                        }
                        if (isDirty)
                        {
                            AssetDatabase.WriteImportSettingsIfDirty(path);
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        }
                    }
                }
            }
            container.Set(m_OutputPort.Id, assets);
        }
    }
}
