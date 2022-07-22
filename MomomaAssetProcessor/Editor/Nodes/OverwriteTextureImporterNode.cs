using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Object;
using UnityObject = UnityEngine.Object;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [CreateElement(typeof(AssetProcessorGUI), "Importer/Texture")]
    sealed class OverwriteTextureImporterNode : INodeProcessor, IAdditionalAssetHolder
    {
        sealed class OverwriteTextureImporterNodeEditor : INodeProcessorEditor
        {
            [NodeProcessorEditorFactory]
            static void Entry()
            {
                NodeProcessorEditorFactory.EntryEditorFactory<OverwriteTextureImporterNode>((data, serializedPropertyList) => new OverwriteTextureImporterNodeEditor(serializedPropertyList.GetProcessorProperty()));
            }

            readonly SerializedProperty _ImporterProperty;

            Editor m_CachedEditor;

            public bool UseDefaultVisualElement => false;

            OverwriteTextureImporterNodeEditor(SerializedProperty processorProperty)
            {
                _ImporterProperty = processorProperty.FindPropertyRelative(nameof(m_Importer));
                m_CachedEditor = Editor.CreateEditor(_ImporterProperty.objectReferenceValue);
            }

            public void Dispose()
            {
                if (m_CachedEditor != null)
                {
                    DestroyImmediate(m_CachedEditor);
                }
            }

            public void OnGUI()
            {
                if (_ImporterProperty.objectReferenceValue == null)
                    return;
                Editor.CreateCachedEditor(_ImporterProperty.objectReferenceValue, null, ref m_CachedEditor);
                m_CachedEditor.OnInspectorGUI();
            }
        }

        sealed class AssetData
        {
            public static readonly TextureImporter s_DefaultImporter = Resources.Load<TextureImporter>("MomomaAssetProcessor/TextureImporter");
        }

        OverwriteTextureImporterNode() { }

        [SerializeField]
        TextureImporter m_Importer = null;

        public IEnumerable<UnityObject> Assets
        {
            get
            {
                if (m_Importer == null)
                {
                    m_Importer = Instantiate(AssetData.s_DefaultImporter);
                    m_Importer.name = AssetData.s_DefaultImporter.name;
                    m_Importer.hideFlags = HideFlags.HideInHierarchy;
                }
                return new[] { m_Importer };
            }
        }

        public void OnClone()
        {
            foreach (TextureImporter i in this.CloneAssets())
                m_Importer = i;
        }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroupPortDefinition.Default);
            if (m_Importer != null)
            {
                foreach (var assets in assetGroup)
                {
                    if (assets.Importer is TextureImporter importer)
                    {
                        using (var srcSO = new SerializedObject(m_Importer))
                        using (var iterator = srcSO.GetIterator())
                        using (var dstSO = new SerializedObject(importer))
                        {
                            iterator.Next(true);
                            var excludePaths = new HashSet<string>() { "m_Name", "m_UsedFileIDs", "m_ExternalObjects", "m_Output" };
                            while (true)
                            {
                                if (!excludePaths.Contains(iterator.propertyPath))
                                    dstSO.CopyFromSerializedPropertyIfDifferent(iterator);
                                if (!iterator.Next(false))
                                    break;
                            }
                            if (dstSO.hasModifiedProperties)
                            {
                                dstSO.ApplyModifiedPropertiesWithoutUndo();
                                AssetDatabase.WriteImportSettingsIfDirty(assets.AssetPath);
                                importer.SaveAndReimport();
                            }
                        }
                    }
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
