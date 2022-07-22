using System;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Find/Missing Reference")]
    sealed class FindMissingReferenceNode : INodeProcessor
    {
        FindMissingReferenceNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default, "Found Assets");
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroupPortDefinition.Default);
            var foundAssets = new AssetGroup();
            foreach (var assets in assetGroup)
            {
                if (FindMissingReference(assets))
                    foundAssets.Add(assets);
            }
            container.Set(portDataContainer.OutputPorts[0], foundAssets);
        }

        static bool FindMissingReference(AssetData assetData)
        {
            var found = false;
            foreach (var target in assetData.AllAssets)
            {
                using (var so = new SerializedObject(target))
                using (var sp = so.GetIterator())
                {
                    sp.Next(true);
                    while (true)
                    {
                        if (sp.propertyType == SerializedPropertyType.Generic)
                        {
                            if (sp.isArray && sp.arraySize > 0)
                            {
                                var element = sp.GetArrayElementAtIndex(0);
                                if (element.propertyType != SerializedPropertyType.Generic && element.propertyType != SerializedPropertyType.ObjectReference && !element.isArray)
                                {
                                    if (!sp.Next(false))
                                        break;
                                }
                            }
                            if (!sp.Next(true))
                                break;
                        }
                        else
                        {
                            if (sp.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                var value = sp.objectReferenceValue;
                                if (value == null && sp.objectReferenceInstanceIDValue != 0)
                                {
                                    found = true;
                                    Debug.LogWarning($"Missing reference : {target.name}({target.GetType().FullName}), {assetData.AssetPath}, {sp.propertyPath}", target);
                                }
                            }
                            if (!sp.Next(false))
                                break;
                        }
                    }
                }
            }
            return found;
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
