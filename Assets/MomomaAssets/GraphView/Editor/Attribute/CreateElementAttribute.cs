using System;

//#nullable enable

namespace MomomaAssets.GraphView
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CreateElementAttribute : Attribute
    {
        public Type GraphType { get; }
        public string MenuPath { get; }

        public CreateElementAttribute(Type graphType, string menuPath)
        {
            GraphType = graphType;
            MenuPath = menuPath;
        }
    }
}
