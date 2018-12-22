namespace PrefixExpressions
{
    public abstract class ValueNode : INode
    {
        public abstract T Accept<T>(IVisitor<T> visitor);
    }

    /// <summary>
    /// Node representing a constant number literal
    /// </summary>
    public class ConstantNode : ValueNode
    {
        public int Value { get; set; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}