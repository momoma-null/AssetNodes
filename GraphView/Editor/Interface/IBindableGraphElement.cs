using UnityEditor;

//#nullable enable

namespace MomomaAssets.GraphView
{
    interface IBindableGraphElement
    {
        IGraphElementData GraphElementData { get; }
        void Bind(SerializedObject serializedObject);
    }
}
