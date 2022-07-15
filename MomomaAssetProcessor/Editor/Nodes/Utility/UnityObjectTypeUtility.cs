using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView.AssetProcessor
{
    static class UnityObjectTypeUtility
    {
        static readonly MethodInfo s_ScriptingWrapperTypeNameForNativeIDInfo = Type.GetType("UnityEditor.RuntimeClassMetadataUtils, UnityEditor.dll").GetMethod("ScriptingWrapperTypeNameForNativeID", BindingFlags.Static | BindingFlags.Public);

        static class ComponentCommand
        {
            static public string[] DisplayNames { get; }
            static public string[] DisplayNamesWithTransform { get; }
            static public IReadOnlyDictionary<string, int> MenuPaths { get; }
            static public IReadOnlyDictionary<string, string> CommandToMenuPaths { get; }
            static public IReadOnlyDictionary<string, string> MenuPathToCommands { get; }

            static ComponentCommand()
            {
                var removeCount = "Component/".Length;
                var menus = Unsupported.GetSubmenus("Component");
                var commands = Unsupported.GetSubmenusCommands("Component");
                var dstMenus = new List<string>(menus.Length);
                var menuPaths = new Dictionary<string, int>(menus.Length);
                var commandToMenuPaths = new Dictionary<string, string>(menus.Length);
                var menuPathToCommands = new Dictionary<string, string>(menus.Length);
                for (var i = 0; i < menus.Length; ++i)
                {
                    if (commands[i] != "ADD")
                    {
                        var dstPath = menus[i].Remove(0, removeCount);
                        menuPaths.Add(dstPath, menuPaths.Count);
                        dstMenus.Add(dstPath);
                        commandToMenuPaths.Add(commands[i], dstPath);
                        menuPathToCommands.Add(dstPath, commands[i]);
                    }
                }
                DisplayNames = dstMenus.ToArray();
                dstMenus.Insert(0, "Transfrom");
                menuPaths.Add(dstMenus[0], -1);
                DisplayNamesWithTransform = dstMenus.ToArray();
                MenuPaths = menuPaths;
                CommandToMenuPaths = commandToMenuPaths;
                MenuPathToCommands = menuPathToCommands;
            }
        }

        public static string ComponentTypePopup(string menuPath, bool includingTransform = false)
        {
            ComponentCommand.MenuPaths.TryGetValue(menuPath, out var index);
            if (includingTransform)
            {
                ++index;
                index = EditorGUILayout.Popup(index, ComponentCommand.DisplayNamesWithTransform);
                return ComponentCommand.DisplayNamesWithTransform[index];
            }
            else
            {
                index = EditorGUILayout.Popup(index, ComponentCommand.DisplayNames);
                return ComponentCommand.DisplayNames[index];
            }
        }

        public static bool TryGetComponentTypeFromMenuPath(string menuPath, out Type componentType)
        {
            if (ComponentCommand.MenuPathToCommands.TryGetValue(menuPath, out var command))
            {
                if (command.StartsWith("SCRIPT"))
                {
                    var scriptId = int.Parse(command.Substring(6));
                    if (EditorUtility.InstanceIDToObject(scriptId) is MonoScript monoScript)
                        componentType = monoScript.GetClass();
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    var classId = int.Parse(command);
                    if (s_ScriptingWrapperTypeNameForNativeIDInfo.Invoke(null, new object[] { classId }) is string typeName)
                        componentType = Type.GetType($"{typeName}, UnityEngine.dll");
                    else
                        throw new InvalidOperationException();
                }
                return true;
            }
            else
            {
                componentType = typeof(Transform);
                return menuPath == ComponentCommand.DisplayNamesWithTransform[0];
            }
        }

        public static string GetMenuPath(MonoScript monoScript)
        {
            if (ComponentCommand.CommandToMenuPaths.TryGetValue($"SCRIPT{monoScript.GetInstanceID()}", out var menuPath))
                return menuPath;
            throw new InvalidOperationException();
        }

        public static string GetMenuPath(int classId)
        {
            if (ComponentCommand.CommandToMenuPaths.TryGetValue(classId.ToString(), out var menuPath))
                return menuPath;
            return ComponentCommand.DisplayNamesWithTransform[0];
        }
    }
}
