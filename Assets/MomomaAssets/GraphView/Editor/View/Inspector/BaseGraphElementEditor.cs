using System;
using System.Collections.Generic;
using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    abstract class BaseGraphElementEditor : IDisposable
    {
        protected BaseGraphElementEditor() { }
        public virtual bool UseDefaultVisualElement => false;
        public virtual void Dispose() { }
        public abstract void OnGUI();
    }

    sealed class DefaultGraphElementEditor : BaseGraphElementEditor
    {
        readonly IReadOnlyList<SerializedProperty> _Properties;

        public override bool UseDefaultVisualElement => true;

        public DefaultGraphElementEditor(SerializedProperty property)
        {
            var properties = new List<SerializedProperty>();
            using (var endProperty = property.GetEndProperty(false))
            {
                if (property.NextVisible(true))
                {
                    while (true)
                    {
                        if (SerializedProperty.EqualContents(property, endProperty))
                            break;
                        properties.Add(property.Copy());
                        if (!property.NextVisible(false))
                            break;
                    }
                }
            }
            _Properties = properties;
        }

        public override void OnGUI()
        {
            foreach (var property in _Properties)
                EditorGUILayout.PropertyField(property, true);
        }
    }
}
