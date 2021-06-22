using UnityEditor.Experimental.GraphView;

namespace MomomaAssets.GraphView
{
    public sealed class TokenNode<T> : TokenNode where T : Edge, new()
    {
        public TokenNode(Port input, Port output) : base(input, output) { }
    }
}
