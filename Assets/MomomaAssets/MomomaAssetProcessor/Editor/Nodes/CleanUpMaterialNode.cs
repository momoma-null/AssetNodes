using System;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Clean up/Material")]
    sealed class CleanUpMaterialNode : INodeProcessor
    {
        static CleanUpMaterialNode()
        {
            INodeDataUtility.AddConstructor(() => new CleanUpMaterialNode());
        }

        CleanUpMaterialNode() { }

        public INodeProcessorEditor ProcessorEditor => new DefaultNodeProcessorEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<Material>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            foreach (var asset in assetGroup)
            {
                if (asset.MainAsset is Material mat)
                {
                    using (var so = new SerializedObject(mat))
                    using (var savedProp = so.FindProperty("m_SavedProperties"))
                    {
                        RemoveProperties(savedProp.FindPropertyRelative("m_TexEnvs"), mat);
                        RemoveProperties(savedProp.FindPropertyRelative("m_Floats"), mat);
                        RemoveProperties(savedProp.FindPropertyRelative("m_Colors"), mat);
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }
        }

        static void RemoveProperties(SerializedProperty props, Material mat)
        {
            for (var i = props.arraySize - 1; i >= 0; --i)
            {
                var name = props.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
                if (!mat.HasProperty(name))
                    props.DeleteArrayElementAtIndex(i);
            }
        }
    }
}
