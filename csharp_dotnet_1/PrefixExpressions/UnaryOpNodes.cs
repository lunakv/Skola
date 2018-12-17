namespace PrefixExpressions
{
    public abstract class UnaryOpNode : OperatorNode
    {
        public override int Arity => 1;
        public INode Son { get; set; }

        public override bool AddSon(INode son)
        {
            Son = Son ?? son;
            return true;
        }
    }

    public class NegateOpNode : UnaryOpNode
    {
        public override char Operator => '~';
        
        public override int IntEvaluate(IntAlgorithm alg)
        {
            return alg.Call(this);
        }

        public override double DoubleEvaluate(IDoubleAlgorithm alg)
        {
            return alg.Call(this);
        }
    }
}