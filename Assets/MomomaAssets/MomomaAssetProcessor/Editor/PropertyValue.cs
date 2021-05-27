
namespace MomomaAssets.AssetProcessor
{
    abstract class PropertyValue
    {
        public static PropertyValue<T> Create<T>(T defaultValue = default(T)) => new PropertyValue<T>() { Value = defaultValue };
    }

    sealed class PropertyValue<T> : PropertyValue
    {
        public T Value { get; set; }
    }
}
