using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;
using static UnityEngine.Object;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [InitializeOnLoad]
    [Serializable]
    sealed class ComponentModifierNode : INodeData
    {
        static ComponentModifierNode()
        {
            INodeDataUtility.AddConstructor(() => new ComponentModifierNode());
        }

        public string Title => "Modifiy Component";
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
        bool m_Recursively;
        [SerializeField]
        string m_TypeName = "";
        [SerializeField]
        string m_SerializedPrefabInstance = "";

        public void Process(ProcessingDataContainer container)
        {
            var assets = container.Get(m_InputPort.Id, () => new AssetGroup());
            foreach (var asset in assets)
            {
                if (asset is GameObject target)
                {

                }
            }
        }

        [CustomPropertyDrawer(typeof(ComponentModifierNode))]
        sealed class ComponentModifierNodeDrawer : PropertyDrawer
        {
            Editor? m_CachedEditor;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                using (var m_TypeNameProperty = property.FindPropertyRelative(nameof(m_TypeName)))
                using (var m_SerializedPrefabInstanceProperty = property.FindPropertyRelative(nameof(m_SerializedPrefabInstance)))
                {
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var newValue = ComponentReflectionUtility.TypePopup(position, m_TypeNameProperty.stringValue);
                        if (change.changed)
                            m_TypeNameProperty.stringValue = newValue;
                    }
                    var component = ComponentReflectionUtility.GetInstanceComponent(m_TypeNameProperty.stringValue);
                    if (component != null)
                    {
                        if (string.IsNullOrEmpty(m_SerializedPrefabInstanceProperty.stringValue))
                            m_SerializedPrefabInstanceProperty.stringValue = ComponentReflectionUtility.GetInitialVariantPrefab();
                        ComponentReflectionUtility.DeserializeVariantPrefab(m_SerializedPrefabInstanceProperty.stringValue);
                        Editor.CreateCachedEditor(component, null, ref m_CachedEditor);
                        EditorGUI.BeginChangeCheck();
                        m_CachedEditor.OnInspectorGUI();
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_SerializedPrefabInstanceProperty.stringValue = ComponentReflectionUtility.SerializeVariantPrefab();
                        }
                    }
                }
            }
        }
    }
}
