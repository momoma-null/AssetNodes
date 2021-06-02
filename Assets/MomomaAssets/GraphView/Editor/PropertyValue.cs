using System.Collections;

#nullable enable

namespace MomomaAssets.GraphView
{
    public abstract class PropertyValue
    {
        public static PropertyValue<T> Create<T>(string fieldName, T value) => new PropertyValue<T>(fieldName, value);

        public string FieldName { get; }

        public PropertyValue(string fieldName)
        {
            FieldName = fieldName;
        }
    }

    public sealed class PropertyValue<T> : PropertyValue
    {
        public T Value { get; set; }
        
        public PropertyValue(string fieldName, T value) : base(fieldName)
        {
            Value = value;
        }
    }

    public sealed class PropertyValueList : PropertyValue
    {
        public IList Value { get; set; }

        public PropertyValueList(string fieldName, IList value) : base(fieldName)
        {
            Value = value;
        }
    }
}
