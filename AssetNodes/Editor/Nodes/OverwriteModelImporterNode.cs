using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Object;
using UnityObject = UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetNodes
{
    [Serializable]
    [CreateElement(typeof(AssetNodesGUI), "Importer/Model")]
    sealed class OverwriteModelImporterNode : INodeProcessor, IAdditionalAssetHolder
    {
        sealed class OverwriteModelImporterNodeEditor : INodeProcessorEditor
        {
            [NodeProcessorEditorFactory]
            static void Entry()
            {
                NodeProcessorEditorFactory.EntryEditorFactory<OverwriteModelImporterNode>((data, serializedPropertyList) => new OverwriteModelImporterNodeEditor(serializedPropertyList.GetProcessorProperty()));
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

            static Editor CreateEditorIfNecessary(UnityObject target, Editor? currentEditor)
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
            public static readonly ModelImporter s_DefaultImporter = Resources.Load<ModelImporter>("AssetNodes/ModelImporter");
        }

        OverwriteModelImporterNode() { }

        [SerializeField]
        ModelImporter? m_Importer = null;

        public Color HeaderColor => ColorDefinition.ImporterNode;

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
            portDataContainer.AddInputPort(AssetGroupPortDefinition.Default);
            portDataContainer.AddOutputPort(AssetGroupPortDefinition.Default);
        }

        public void Process(IProcessingDataContainer container)
        {
            var assetGroup = container.GetInput(0, AssetGroupPortDefinition.Default);
            if (m_Importer != null)
            {
                var excludePaths = new HashSet<string>() { "m_RigImportErrors", "m_ImportedTakeInfos", "m_ImportedRoots", "m_HasExtraRoot" };
                foreach (var assets in assetGroup)
                {
                    if (assets.Importer is ModelImporter importer)
                    {
                        using (var srcSO = new SerializedObject(m_Importer))
                        using (var iterator = srcSO.GetIterator())
                        using (var dstSO = new SerializedObject(importer))
                        {
                            iterator.NextVisible(true);
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
                                AssetDatabase.WriteImportSettingsIfDirty(assets.AssetPath);
                                importer.SaveAndReimport();
                            }
                        }
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
