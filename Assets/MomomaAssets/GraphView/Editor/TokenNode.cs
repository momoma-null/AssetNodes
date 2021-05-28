using UnityEditor.Experimental.GraphView;

namespace MomomaAssets.GraphView
{
    public sealed class TokenNode<T> : TokenNode where T : Edge, new()
    {
        TokenNode() : this(
            Port.Create<T>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, null),
            Port.Create<T>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, null))
        { }

        public TokenNode(Port input, Port output) : base(input, output) { }
    }
}
