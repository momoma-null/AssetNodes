using UnityEngine.UIElements;

namespace MomomaAssets.GraphView
{
    public interface IGraphViewCallback
    {
        void Initialize();
        void OnValueChanged(VisualElement visualElement);
    }
}
