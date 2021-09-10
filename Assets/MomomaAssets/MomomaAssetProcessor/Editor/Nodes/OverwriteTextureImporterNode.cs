using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Importer/Texture")]
    sealed class OverwriteTextureImporterNode : INodeProcessor, IAdditionalAssetHolder
    {
        sealed class OverwriteTextureImporterNodeEditor : INodeProcessorEditor
        {
            readonly OverwriteTextureImporterNode m_Node;

            Editor? m_CachedEditor;

            public bool UseDefaultVisualElement => false;

            public OverwriteTextureImporterNodeEditor(OverwriteTextureImporterNode node)
            {
                m_Node = node;
            }

            public void OnEnable()
            {
                if (m_CachedEditor == null)
                {
                    m_CachedEditor = Editor.CreateEditor(m_Node.m_Importer);
                }
            }

            public void OnDisable()
            {
                if (m_CachedEditor != null)
                {
                    DestroyImmediate(m_CachedEditor);
                    m_CachedEditor = null;
                }
            }

            public void OnGUI(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty)
            {
                using (var m_ImporterProperty = processorProperty.FindPropertyRelative(nameof(m_Importer)))
                {
                    if (m_ImporterProperty.objectReferenceValue == null)
                        return;
                    Editor.CreateCachedEditor(m_ImporterProperty.objectReferenceValue, null, ref m_CachedEditor);
                    m_CachedEditor.OnInspectorGUI();
                }
            }
        }

        sealed class AssetData
        {
            public static readonly TextureImporter s_DefaultImporter = Resources.Load<TextureImporter>("MomomaAssetProcessor/TextureImporter");
        }

        static OverwriteTextureImporterNode()
        {
            INodeDataUtility.AddConstructor(() => new OverwriteTextureImporterNode());
        }

        OverwriteTextureImporterNode() { }

        [SerializeField]
        TextureImporter? m_Importer = null;

        OverwriteTextureImporterNodeEditor? m_Editor;

        public INodeProcessorEditor ProcessorEditor => m_Editor ?? (m_Editor = new OverwriteTextureImporterNodeEditor(this));
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
            portDataContainer.AddInputPort<Texture>(isMulti: true);
            portDataContainer.AddOutputPort<Texture>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
            if (m_Importer != null)
            {
                foreach (var assets in assetGroup)
                {
                    var path = assets.AssetPath;
                    if (AssetImporter.GetAtPath(path) is TextureImporter importer)
                    {
                        using (var srcSO = new SerializedObject(m_Importer))
                        using (var iterotor = srcSO.GetIterator())
                        using (var dstSO = new SerializedObject(importer))
                        {
                            iterotor.Next(true);
                            var excludePaths = new HashSet<string>() { "m_Name", "m_UsedFileIDs", "m_ExternalObjects", "m_Output" };
                            while (true)
                            {
                                if (!excludePaths.Contains(iterotor.propertyPath))
                                    dstSO.CopyFromSerializedPropertyIfDifferent(iterotor);
                                if (!iterotor.Next(false))
                                    break;
                            }
                            if (dstSO.hasModifiedProperties)
                            {
                                dstSO.ApplyModifiedPropertiesWithoutUndo();
                                AssetDatabase.WriteImportSettingsIfDirty(path);
                                importer.SaveAndReimport();
                            }
                        }
                    }
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
