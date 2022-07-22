using System;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Clean up/Material")]
    sealed class CleanUpMaterialNode : INodeProcessor
    {
        CleanUpMaterialNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroupPortDefinition.Default);
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

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
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
