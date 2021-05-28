using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace MomomaAssets.GraphView
{

    static class UIElementsUtility
    {
        internal static VisualElement CreateLabeledElement(string lablel, VisualElement element)
        {
            var horizontalElement = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
            element.style.flexGrow = 1f;
            horizontalElement.Add(new Label(lablel));
            horizontalElement.Add(element);
            return horizontalElement;
        }
    }

    public sealed class SliderWithFloatField : BaseField<float>
    {
        readonly FloatField m_FloatField;
        readonly Slider m_Slider;

        public SliderWithFloatField(string label, float start, float end, float initial) : base(label, null)
        {
            m_Slider = new Slider(start, end) { style = { flexGrow = 1f } };
            m_FloatField = new FloatField() { style = { flexGrow = 0.8f, flexShrink = 1f } };
            m_Slider.RegisterValueChangedCallback(e => e.target = this);
            m_Slider.RegisterValueChangedCallback(e => value = e.newValue);
            m_FloatField.RegisterValueChangedCallback(e => e.target = this);
            m_FloatField.RegisterValueChangedCallback(e => value = e.newValue);
            this.RegisterValueChangedCallback(e => m_Slider.value = e.newValue);
            this.RegisterValueChangedCallback(e => m_FloatField.value = e.newValue);
            value = initial;
            style.flexDirection = FlexDirection.Row;
            Add(m_Slider);
            Add(m_FloatField);
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(newValue);
            m_Slider.SetValueWithoutNotify(newValue);
            m_FloatField.SetValueWithoutNotify(newValue);
        }
    }

    public sealed class EnumPopupField<T> : PopupField<T> where T : struct, Enum
    {
        public EnumPopupField(string label, T defaultValue)
         : base(label, Enum.GetNames(typeof(T)).Select(i => { Enum.TryParse<T>(i, out var result); return result; }).ToList(), defaultValue) { }
    }

}
