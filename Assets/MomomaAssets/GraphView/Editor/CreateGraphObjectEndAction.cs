using System;
using UnityEditor.ProjectWindowCallback;

namespace MomomaAssets.GraphView
{
    sealed class CreateGraphObjectEndAction : EndNameEditAction
    {
        public event Action<string> OnEndNameEdit;

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            OnEndNameEdit?.Invoke(pathName);
        }
    }
}
