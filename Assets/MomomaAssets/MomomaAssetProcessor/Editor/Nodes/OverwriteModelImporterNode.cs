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
    [CreateElement("Importer/Model")]
    sealed class OverwriteModelImporterNode : INodeProcessor, IAdditionalAssetHolder
    {
        sealed class OverwriteModelImporterNodeEditor : INodeProcessorEditor
        {
            readonly OverwriteModelImporterNode m_Node;

            Editor? m_CachedEditor;

            public bool UseDefaultVisualElement => false;

            public OverwriteModelImporterNodeEditor(OverwriteModelImporterNode node)
            {
                m_Node = node;
            }

            public void OnEnable()
            {
                if (m_CachedEditor == null)
                {
                    CreateEditorIfNecessary(m_Node.m_Importer);
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
                    CreateEditorIfNecessary(m_ImporterProperty.objectReferenceValue);
                    m_CachedEditor?.OnInspectorGUI();
                }
            }

            void CreateEditorIfNecessary(UnityObject? target)
            {
                if (target == null)
                    return;
                if (m_CachedEditor != null)
                {
                    if (m_CachedEditor.target == target)
                        return;
                    DestroyImmediate(m_CachedEditor);
                }
                var delegates = Undo.undoRedoPerformed.GetInvocationList();
                m_CachedEditor = Editor.CreateEditor(target);
                Undo.undoRedoPerformed = null;
                foreach (var i in delegates)
                    Undo.undoRedoPerformed += i as Undo.UndoRedoCallback;

            }
        }

        sealed class AssetData
        {
            public static readonly ModelImporter s_DefaultImporter = Resources.Load<ModelImporter>("MomomaAssetProcessor/ModelImporter");
        }

        static OverwriteModelImporterNode()
        {
            INodeDataUtility.AddConstructor(() => new OverwriteModelImporterNode());
        }

        OverwriteModelImporterNode() { }

        [SerializeField]
        ModelImporter? m_Importer = null;

        OverwriteModelImporterNodeEditor? m_Editor;

        public INodeProcessorEditor ProcessorEditor => m_Editor ?? (m_Editor = new OverwriteModelImporterNodeEditor(this));
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
            foreach (ModelImporter i in this.CloneAssets())
                m_Importer = i;
        }

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(GameObject)));
            portDataContainer.OutputPorts.Add(new PortData(typeof(GameObject)));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], this.NewAssetGroup, this.CopyAssetGroup);
            if (m_Importer != null)
            {
                foreach (var assets in assetGroup)
                {
                    var path = assets.AssetPath;
                    if (AssetImporter.GetAtPath(path) is ModelImporter importer)
                    {
                        using (var srcSO = new SerializedObject(m_Importer))
                        using (var iterator = srcSO.GetIterator())
                        using (var dstSO = new SerializedObject(importer))
                        {
                            iterator.NextVisible(true);
                            var excludePaths = new HashSet<string>() { "m_RigImportErrors", "m_ImportedTakeInfos", "m_ImportedRoots", "m_HasExtraRoot" };
                            while (true)
                            {
                                if (iterator.editable && !excludePaths.Contains(iterator.propertyPath))
                                    dstSO.CopyFromSerializedPropertyIfDifferent(iterator);
                                if (!iterator.NextVisible(false))
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
