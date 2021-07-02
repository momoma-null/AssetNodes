using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [Serializable]
    sealed class OverwriteTextureImporterNode : INodeData
    {
        sealed class OverwriteTextureImporterNodeEditor : IGraphElementEditor
        {
            static class DefaultImporter
            {
                public static readonly Texture s_DefaultTexture = Resources.Load<Texture>("DefaultTexture");
                public static readonly AssetImporter s_DefaultImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(s_DefaultTexture));
                public static readonly string s_InitialImporter = EditorJsonUtility.ToJson(s_DefaultImporter);
            }

            [Serializable]
            sealed class PropertyData
            {
                static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> s_PropertyInfos = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

                [SerializeReference]
                List<object> m_SerializedValues = new List<object>();
                [SerializeField]
                List<string> m_PropertyNames = new List<string>();

                public void SerializeProperties(object target)
                {
                    var targetType = target.GetType();
                    if (!s_PropertyInfos.TryGetValue(targetType, out var infos))
                    {
                        infos = new Dictionary<string, PropertyInfo>();
                        s_PropertyInfos.Add(targetType, infos);
                        foreach (var info in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                            if (info.CanRead && info.CanWrite)
                                infos[info.Name] = info;
                    }
                    m_SerializedValues = new List<object>();
                    m_PropertyNames = new List<string>();
                    foreach (var info in infos)
                    {
                        m_SerializedValues.Add(info.Value.GetValue(target));
                        m_PropertyNames.Add(info.Key);
                    }
                }
            }

            Editor? m_CachedEditor;

            public bool UseDefaultVisualElement => false;
            public void OnDestroy() { }
            public void OnGUI(SerializedProperty property)
            {
                using (var m_SerializedImporterProperty = property.FindPropertyRelative(nameof(m_SerializedImporter)))
                {
                    Editor.CreateCachedEditor(DefaultImporter.s_DefaultImporter, null, ref m_CachedEditor);
                    if (!string.IsNullOrEmpty(m_SerializedImporterProperty.stringValue))
                    {
                        var temp = Instantiate(DefaultImporter.s_DefaultImporter);
                        Debug.Log(m_SerializedImporterProperty.stringValue);
                        EditorJsonUtility.FromJsonOverwrite(m_SerializedImporterProperty.stringValue, temp);
                        Debug.Log(EditorJsonUtility.ToJson(temp));
                        DestroyImmediate(temp);
                    }
                    using (var tempSO = new SerializedObject(DefaultImporter.s_DefaultImporter))
                    {
                        EditorGUI.BeginChangeCheck();
                        m_CachedEditor.OnInspectorGUI();
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_CachedEditor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                            m_SerializedImporterProperty.stringValue = EditorJsonUtility.ToJson(DefaultImporter.s_DefaultImporter);
                            using (var sp = tempSO.GetIterator())
                            {
                                sp.Next(true);
                                while (true)
                                {
                                    m_CachedEditor.serializedObject.CopyFromSerializedPropertyIfDifferent(sp);
                                    if (!sp.Next(false))
                                        break;
                                }
                            }
                            m_CachedEditor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        }
                    }
                }
            }
        }

        static OverwriteTextureImporterNode()
        {
            INodeDataUtility.AddConstructor(() => new OverwriteTextureImporterNode());
        }

        public IGraphElementEditor GraphElementEditor { get; } = new OverwriteTextureImporterNodeEditor();
        public string Title => "Overwirte Texture Importer";
        public string MenuPath => "Importer/TextureImporter";
        public IEnumerable<PortData> InputPorts => new[] { m_InputPort };
        public IEnumerable<PortData> OutputPorts => new[] { m_OutputPort };

        [SerializeField]
        [HideInInspector]
        PortData m_InputPort = new PortData(typeof(Texture));

        [SerializeField]
        [HideInInspector]
        PortData m_OutputPort = new PortData(typeof(Texture));

        [SerializeField]
        string m_SerializedImporter = "";

        public void Process(ProcessingDataContainer container)
        {
            var assets = container.Get(m_InputPort.Id, () => new AssetGroup());
            foreach (var asset in assets)
            {
                if (asset is Texture texture)
                {
                    var path = AssetDatabase.GetAssetPath(texture);
                    if (AssetImporter.GetAtPath(path) is TextureImporter importer)
                    {
                        EditorJsonUtility.FromJsonOverwrite(m_SerializedImporter, importer);
                        importer.SaveAndReimport();
                    }
                }
            }
        }
    }
}
