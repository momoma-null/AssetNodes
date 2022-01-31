using System;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    public sealed class SerializedPropertyList
    {
        readonly SerializedProperty _PortsProperty;

        public int Count => _PortsProperty.arraySize;

        internal SerializedPropertyList(SerializedProperty property)
        {
            if (!property.isArray)
                throw new InvalidOperationException($"{nameof(property)} is not array");
            _PortsProperty = property;
        }

        public void Add() => ++_PortsProperty.arraySize;

        public void Move(int srcIndex, int dstIndex)=>_PortsProperty.MoveArrayElement(srcIndex, dstIndex);

        public void RemoveAt(int index) => _PortsProperty.DeleteArrayElementAtIndex(index);

        internal SerializedProperty GetElementAtIndex(int index) => _PortsProperty.GetArrayElementAtIndex(index);
    }
}
