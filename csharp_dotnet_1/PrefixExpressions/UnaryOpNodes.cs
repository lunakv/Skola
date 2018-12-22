namespace PrefixExpressions
{
    public abstract class UnaryOpNode : OperatorNode
    {
        public override int Arity => 1;
        public INode Son { get; private set; }

        public override bool AddSon(INode son)
        {
            Son = Son ?? son;
            return true;
        }
    }

    public class NegateOpNode : UnaryOpNode
    {
        public override char Operator => '~';

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}