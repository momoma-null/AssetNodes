using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    [Serializable]
    [InitializeOnLoad]
    [CreateElement("Modify/Add Component")]
    sealed class AddComponentNode : INodeProcessor
    {
        sealed class AddComponentNodeEditor : INodeProcessorEditor
        {
            static readonly Dictionary<string, int> s_MenuPaths = new Dictionary<string, int>();
            static string[] s_displayNames = new string[0];

            public static IReadOnlyDictionary<string, int> MenuPaths => s_MenuPaths;

            public bool UseDefaultVisualElement => false;

            public void OnDestroy() { }

            public void OnGUI(SerializedProperty processorProperty, SerializedProperty inputPortsProperty, SerializedProperty outputPortsProperty)
            {
                if (s_MenuPaths.Count == 0)
                {
                    var removeCount = "Component/".Length;
                    var menus = Unsupported.GetSubmenus("Component");
                    var commands = Unsupported.GetSubmenusCommands("Component");
                    var dstMenus = new List<string>();
                    for (var i = 0; i < menus.Length; ++i)
                    {
                        if (commands[i] != "ADD")
                        {
                            var dstPath = menus[i].Remove(0, removeCount);
                            s_MenuPaths.Add(dstPath, s_MenuPaths.Count);
                            dstMenus.Add(dstPath);
                        }
                    }
                    s_displayNames = dstMenus.ToArray();
                }
                using (var m_IncludeChildrenProperty = processorProperty.FindPropertyRelative(nameof(m_IncludeChildren)))
                using (var m_RegexProperty = processorProperty.FindPropertyRelative(nameof(m_Regex)))
                using (var m_MenuPathProperty = processorProperty.FindPropertyRelative(nameof(m_MenuPath)))
                {
                    EditorGUILayout.PropertyField(m_IncludeChildrenProperty);
                    EditorGUILayout.PropertyField(m_RegexProperty);
                    EditorGUI.BeginChangeCheck();
                    s_MenuPaths.TryGetValue(m_MenuPathProperty.stringValue, out var index);
                    index = EditorGUILayout.Popup(index, s_displayNames);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_MenuPathProperty.stringValue = s_displayNames[index];
                    }
                }
            }
        }

        static readonly MethodInfo s_ExecuteMenuItemOnGameObjectsInfo = typeof(EditorApplication).GetMethod("ExecuteMenuItemOnGameObjects", BindingFlags.Static | BindingFlags.NonPublic);

        static AddComponentNode()
        {
            INodeDataUtility.AddConstructor(() => new AddComponentNode());
        }

        AddComponentNode() { }

        [SerializeField]
        bool m_IncludeChildren = false;
        [SerializeField]
        string m_Regex = "";
        [SerializeField]
        string m_MenuPath = "";

        public INodeProcessorEditor ProcessorEditor { get; } = new AddComponentNodeEditor();

        public void Initialize(IPortDataContainer portDataContainer)
        {
            portDataContainer.InputPorts.Add(new PortData(typeof(GameObject)));
            portDataContainer.OutputPorts.Add(new PortData(typeof(GameObject)));
        }

        public void Process(ProcessingDataContainer container, IPortDataContainer portDataContainer)
        {
            var assetGroup = container.Get(portDataContainer.InputPorts[0], this.NewAssetGroup);
            if (AddComponentNodeEditor.MenuPaths.TryGetValue(m_MenuPath, out var index))
            {
                Type componentType;
                var command = Unsupported.GetSubmenusCommands("Component")[index];
                if (command.StartsWith("SCRIPT"))
                {
                    var scriptId = int.Parse(command.Substring(6));
                    var monoScript = EditorUtility.InstanceIDToObject(scriptId) as MonoScript;
                    componentType = monoScript?.GetClass() ?? throw new InvalidOperationException();
                }
                else
                {
                    var classId = int.Parse(command);
                    componentType = UnityObjectTypeUtility.GetTypeFromClassId(classId);
                }
                var regex = new Regex(m_Regex);
                var menuPath = "Component/" + m_MenuPath;
                foreach (var assets in assetGroup)
                {
                    if ((assets.MainAsset.hideFlags & HideFlags.NotEditable) != 0)
                        continue;
                    if (m_IncludeChildren)
                    {
                        foreach (var go in assets.GetAssetsFromType<GameObject>())
                        {
                            if (regex.Match(go.name).Success)
                            {
                                if (go.GetComponent(componentType) == null)
                                    go.AddComponent(componentType);
                            }
                        }
                    }
                    else if (assets.MainAsset is GameObject root)
                    {
                        if (regex.Match(root.name).Success)
                        {
                            if (root.GetComponent(componentType) == null)
                                root.AddComponent(componentType);
                        }
                    }
                }
            }
            container.Set(portDataContainer.OutputPorts[0], assetGroup);
        }
    }
}
