using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Object;
using UnityObject = UnityEngine.Object;

#nullable enable

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
        TextureImporter? m_Importer = null;

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

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            if (m_Importer != null)
            {
                var textureSettings = new TextureImporterSettings();
                m_Importer.ReadTextureSettings(textureSettings);
                foreach (var assets in assetGroup)
                {
                    if (assets.Importer is TextureImporter importer)
                    {
                        importer.SetTextureSettings(textureSettings);
                        AssetDatabase.WriteImportSettingsIfDirty(assets.AssetPath);
                    }
                }
            }
            container.SetOutput(0, assetGroup);
        }

        public T DoFunction<T>(IFunctionContainer<INodeProcessor, T> function)
        {
            return function.DoFunction(this);
        }
    }
}
