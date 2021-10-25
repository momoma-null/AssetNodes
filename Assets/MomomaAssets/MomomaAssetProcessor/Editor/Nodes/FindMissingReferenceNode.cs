using System;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Find/Missing Reference")]
    sealed class FindMissingReferenceNode : INodeProcessor
    {
        static FindMissingReferenceNode()
        {
            INodeDataUtility.AddConstructor(() => new FindMissingReferenceNode());
        }

        FindMissingReferenceNode() { }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort<UnityObject>(isMulti: true);
            portDataContainer.AddOutputPort<UnityObject>("Found Assets", true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
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
    }
}
