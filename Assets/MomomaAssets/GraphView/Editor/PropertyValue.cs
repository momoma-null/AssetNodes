
namespace MomomaAssets.GraphView
{
    public abstract class PropertyValue
    {
        public static PropertyValue<T> Create<T>(T defaultValue = default(T)) => new PropertyValue<T>() { Value = defaultValue };
    }

    public sealed class PropertyValue<T> : PropertyValue
    {
        public T Value { get; set; }
    }
}
