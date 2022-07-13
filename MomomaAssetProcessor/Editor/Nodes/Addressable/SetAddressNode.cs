
#if ADDRESSABLES
using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Addressable/Set Address")]
    sealed class SetAddressNode : INodeProcessor
    {
        SetAddressNode() { }

        [SerializeField]
        bool m_CreateAddressable = true;
        [SerializeField]
        string m_GroupName = "";
        [SerializeField]
        bool m_LowerCase = true;

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            var aaSettings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            var group = string.IsNullOrEmpty(m_GroupName) ? aaSettings.DefaultGroup : aaSettings.FindGroup(m_GroupName);
            foreach (var assets in assetGroup)
            {
                var guid = AssetDatabase.AssetPathToGUID(assets.AssetPath);
                if (m_CreateAddressable)
                {
                    var entry = aaSettings.FindAssetEntry(guid);
                    if (entry == null)
                    {
                        entry = aaSettings.CreateOrMoveEntry(guid, group);
                    }
                    entry.address = m_LowerCase ? entry.AssetPath.ToLower() : entry.AssetPath;
                }
                else
                {
                    aaSettings.RemoveAssetEntry(guid);
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
#endif
