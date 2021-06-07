using UnityEditor;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IFieldHolder
    {
        IGraphElementData GraphElementData { get; }
        void Bind(SerializedObject serializedObject);
        void OnValueChanged();
    }
}
