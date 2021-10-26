using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;
using static UnityEngine.Object;

//#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Importer/Model")]
    sealed class OverwriteModelImporterNode : INodeProcessor, IAdditionalAssetHolder
    {
        sealed class OverwriteModelImporterNodeEditor : INodeProcessorEditor
        {
            [NodeProcessorEditorFactory]
            static void Entry(IEntryDelegate<GenerateNodeProcessorEditor> factories)
            {
                factories.Add(typeof(OverwriteModelImporterNode), (data, serializedPropertyList) => new OverwriteModelImporterNodeEditor(serializedPropertyList.GetProcessorProperty()));
            }

            readonly SerializedProperty _ImporterProperty;

            Editor m_CachedEditor;

            public bool UseDefaultVisualElement => false;

            OverwriteModelImporterNodeEditor(SerializedProperty processorProperty)
            {
                _ImporterProperty = processorProperty.FindPropertyRelative(nameof(m_Importer));
                m_CachedEditor = CreateEditorIfNecessary(_ImporterProperty.objectReferenceValue, null);
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
                m_CachedEditor = CreateEditorIfNecessary(_ImporterProperty.objectReferenceValue, m_CachedEditor);
                m_CachedEditor?.OnInspectorGUI();
            }

            static Editor CreateEditorIfNecessary(UnityObject target, Editor currentEditor)
            {
                if (currentEditor != null)
                {
                    if (currentEditor.target == target)
                        return currentEditor;
                    DestroyImmediate(currentEditor);
                }
                var delegates = Undo.undoRedoPerformed.GetInvocationList();
                var newEditor = Editor.CreateEditor(target);
                Undo.undoRedoPerformed = null;
                foreach (var i in delegates)
                    Undo.undoRedoPerformed += i as Undo.UndoRedoCallback;
                return newEditor;
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
        ModelImporter m_Importer = null;

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
            portDataContainer.AddInputPort<GameObject>(isMulti: true);
            portDataContainer.AddOutputPort<GameObject>(isMulti: true);
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], AssetGroup.combineAssetGroup);
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
