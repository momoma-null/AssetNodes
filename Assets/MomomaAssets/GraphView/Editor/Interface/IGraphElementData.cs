using System.Collections.Generic;

#nullable enable

namespace MomomaAssets.GraphView
{
    public interface IGraphElementData
    {
        IEnumerable<PropertyValue> GetProperties();
    }
}
