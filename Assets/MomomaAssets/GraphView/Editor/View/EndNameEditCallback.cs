using System;
using UnityEditor.ProjectWindowCallback;

#nullable enable

namespace MomomaAssets.GraphView
{
    sealed class EndNameEditCallback : EndNameEditAction
    {
        public event Action<string>? OnEndNameEdit;
        public event Action? OnCancelled;

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            OnEndNameEdit?.Invoke(pathName);
        }

        public override void Cancelled(int instanceId, string pathName, string resourceFile)
        {
            OnCancelled?.Invoke();
            base.Cancelled(instanceId, pathName, resourceFile);
        }
    }
}
