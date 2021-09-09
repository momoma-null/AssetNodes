using System;

//#nullable enable

namespace MomomaAssets.GraphView
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CreateElementAttribute : Attribute
    {
        public string MenuPath { get; }

        public CreateElementAttribute(string menuPath)
        {
            MenuPath = menuPath;
        }
    }
}
