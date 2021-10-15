using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Clean up/Model Importer")]
    sealed class CleanUpModelImporterNode : INodeProcessor
    {
        static CleanUpModelImporterNode()
        {
            INodeDataUtility.AddConstructor(() => new CleanUpModelImporterNode());
        }

        CleanUpModelImporterNode() { }

        public INodeProcessorEditor ProcessorEditor => new DefaultNodeProcessorEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<GameObject>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            foreach (var asset in assetGroup)
            {
                if (asset.Importer is ModelImporter)
                {
                    using (var so = new SerializedObject(asset.Importer))
                    using (var m_ExternalObjects = so.FindProperty("m_ExternalObjects"))
                    using (var m_Materials = so.FindProperty("m_Materials"))
                    {
                        var externalObjects = new Dictionary<(string, string), int>();
                        for (var i = 0; i < m_ExternalObjects.arraySize; ++i)
                        {
                            using (var element = m_ExternalObjects.GetArrayElementAtIndex(i))
                                externalObjects.Add((element.FindPropertyRelative("first.name").stringValue, element.FindPropertyRelative("first.type").stringValue), i);
                        }
                        for (var i = 0; i < m_Materials.arraySize; ++i)
                        {
                            using (var element = m_Materials.GetArrayElementAtIndex(i))
                                externalObjects.Remove((element.FindPropertyRelative("name").stringValue, element.FindPropertyRelative("type").stringValue));
                        }
                        var sortedIndices = new SortedSet<int>(externalObjects.Values);
                        foreach (var i in sortedIndices.Reverse())
                        {
                            m_ExternalObjects.DeleteArrayElementAtIndex(i);
                        }
                        if (so.ApplyModifiedPropertiesWithoutUndo())
                            asset.Importer.SaveAndReimport();
                    }
                }
            }
        }
    }
}
