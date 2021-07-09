using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [Serializable]
    sealed class ModifyComponentNode : INodeData
    {
        sealed class PrefabInstance : IDisposable
        {
            static readonly GameObject s_DefaultPrefab = Resources.Load<GameObject>("DefaultPrefab");
            static readonly string s_DefaultPrefabPath = AssetDatabase.GetAssetPath(s_DefaultPrefab);
            static readonly GameObject s_ReadonlyPrefab = Resources.Load<GameObject>("ReadonlyPrefab");
            static readonly string s_ReadonlyPrefabPath = AssetDatabase.GetAssetPath(s_ReadonlyPrefab);
            static readonly GameObject s_VariantPrefab = Resources.Load<GameObject>("VariantPrefab");
            static readonly string s_VariantPrefabPath = AssetDatabase.GetAssetPath(s_VariantPrefab);

            readonly GameObject m_PrefabContents;

            public PrefabInstance()
            {
                var scene = EditorSceneManager.NewPreviewScene();
                PrefabUtility.LoadPrefabContentsIntoPreviewScene(s_VariantPrefabPath, scene);
                m_PrefabContents = scene.GetRootGameObjects()[0];
            }

            ~PrefabInstance() => Dispose();

            public void Dispose()
            {
                if (m_PrefabContents != null)
                {
                    PrefabUtility.UnloadPrefabContents(m_PrefabContents);
                }
            }

            public string Serialize()
            {
                return EditorJsonUtility.ToJson(SerializableWrapper.Create(PrefabUtility.GetPropertyModifications(m_PrefabContents)));
            }

            public void Deserialize(string json)
            {
                if (!string.IsNullOrEmpty(json))
                {
                    var modifications = SerializableWrapper.Create(new PropertyModification[0]);
                    EditorJsonUtility.FromJsonOverwrite(json, modifications);
                    PrefabUtility.SetPropertyModifications(m_PrefabContents, modifications);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(m_PrefabContents);
                }
            }

            public Component? GetComponent(string typeName)
            {
                if (ComponentReflectionUtility.TryGetType(typeName, out var type))
                {
                    var tf = m_PrefabContents.transform.Find(typeName);
                    if (tf == null)
                    {
                        var names = typeName.Split('/');
                        using (var contents = new PrefabUtility.EditPrefabContentsScope(s_DefaultPrefabPath))
                        {
                            var parent = contents.prefabContentsRoot.transform;
                            foreach (var i in names)
                            {
                                tf = parent.Find(i);
                                if (tf == null)
                                    tf = new GameObject(i).transform;
                                tf.SetParent(parent);
                                parent = tf;
                            }
                            if (type != typeof(Transform))
                                tf!.gameObject.AddComponent(type);
                        }
                        AssetDatabase.ImportAsset(s_ReadonlyPrefabPath);
                        tf = m_PrefabContents.transform.Find(typeName);
                    }
                    var component = tf.GetComponent(type);
                    return component;
                }
                return null;
            }

            public List<SerializedProperty> GetModifiedProperties(Component target)
            {
                var modifications = PrefabUtility.GetPropertyModifications(m_PrefabContents);
                var targetSource = PrefabUtility.GetCorrespondingObjectFromSource(target);
                var result = new List<SerializedProperty>();
                var so = new SerializedObject(target);
                foreach (var i in modifications)
                {
                    if (i.target != targetSource)
                        continue;
                    var prop = so.FindProperty(i.propertyPath);
                    if (prop != null)
                        result.Add(prop);
                }
                return result;
            }
        }

        sealed class ModifyComponentNodeEditor : IGraphElementEditor
        {
            PrefabInstance? m_PrefabInstance;
            Editor? m_CachedEditor;

            public bool UseDefaultVisualElement => false;

            public void OnDestroy()
            {
                m_PrefabInstance?.Dispose();
                m_PrefabInstance = null;
                if (m_CachedEditor != null)
                    DestroyImmediate(m_CachedEditor);
            }

            public void OnGUI(SerializedProperty property)
            {
                using (var m_IncludeChildrenProperty = property.FindPropertyRelative(nameof(m_IncludeChildren)))
                using (var m_TypeNameProperty = property.FindPropertyRelative(nameof(m_TypeName)))
                using (var m_SerializedPrefabInstanceProperty = property.FindPropertyRelative(nameof(m_SerializedPrefabInstance)))
                {
                    EditorGUILayout.PropertyField(m_IncludeChildrenProperty);
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var newValue = ComponentReflectionUtility.TypePopup(m_TypeNameProperty.stringValue);
                        if (change.changed)
                            m_TypeNameProperty.stringValue = newValue;
                    }
                    if (m_PrefabInstance == null)
                    {
                        m_PrefabInstance = new PrefabInstance();
                        m_PrefabInstance.Deserialize(m_SerializedPrefabInstanceProperty.stringValue);
                    }
                    var component = m_PrefabInstance.GetComponent(m_TypeNameProperty.stringValue);
                    if (component != null)
                    {
                        Editor.CreateCachedEditor(component, null, ref m_CachedEditor);
                        if (m_CachedEditor.serializedObject.UpdateIfRequiredOrScript())
                            m_SerializedPrefabInstanceProperty.stringValue = m_PrefabInstance.Serialize();
                        EditorGUI.BeginChangeCheck();
                        m_CachedEditor.OnInspectorGUI();
                        if (EditorGUI.EndChangeCheck())
                            m_SerializedPrefabInstanceProperty.stringValue = m_PrefabInstance.Serialize();
                    }
                }
            }
        }

        static ModifyComponentNode()
        {
            INodeDataUtility.AddConstructor(() => new ModifyComponentNode());
        }

        ModifyComponentNode() { }

        public IGraphElementEditor GraphElementEditor { get; } = new ModifyComponentNodeEditor();
        public string MenuPath => "Modify/Component";
        public IEnumerable<PortData> InputPorts => new[] { m_InputPort };
        public IEnumerable<PortData> OutputPorts => new[] { m_OutputPort };

        [SerializeField]
        [HideInInspector]
        PortData m_InputPort = new PortData(typeof(GameObject));
        [SerializeField]
        [HideInInspector]
        PortData m_OutputPort = new PortData(typeof(GameObject));
        [SerializeField]
        bool m_IncludeChildren = false;
        [SerializeField]
        string m_TypeName = "";
        [SerializeField]
        string m_SerializedPrefabInstance = "";

        public void Process(ProcessingDataContainer container)
        {
            var assetGroup = container.Get(m_InputPort.Id, () => new AssetGroup());
            using (var prefabInstance = new PrefabInstance())
            {
                prefabInstance.Deserialize(m_SerializedPrefabInstance);
                var component = prefabInstance.GetComponent(m_TypeName);
                if (component != null)
                {
                    var sourceProperties = prefabInstance.GetModifiedProperties(component);
                    var componentType = component.GetType();
                    foreach (var assets in assetGroup)
                    {
                        if (m_IncludeChildren)
                            foreach (var go in assets.GetAssetsFromType<GameObject>())
                                CopyComponentModifications(go, componentType, sourceProperties);
                        else if (assets.MainAsset is GameObject root)
                            CopyComponentModifications(root, componentType, sourceProperties);
                    }
                }
            }
            container.Set(m_OutputPort.Id, assetGroup);
        }

        void CopyComponentModifications(GameObject go, Type componentType, List<SerializedProperty> sourceProperties)
        {
            var targetComponents = go.GetComponents(componentType);
            if (targetComponents == null || targetComponents.Length == 0)
                return;
            using (var targetSO = new SerializedObject(targetComponents))
            {
                foreach (var prop in sourceProperties)
                    targetSO.CopyFromSerializedPropertyIfDifferent(prop);
                if (targetSO.hasModifiedProperties)
                    targetSO.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
