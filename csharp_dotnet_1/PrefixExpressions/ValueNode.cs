namespace PrefixExpressions
{
    public abstract class ValueNode : INode
    {
        public abstract int IntEvaluate(IntAlgorithm alg);
        public abstract double DoubleEvaluate(IDoubleAlgorithm alg);
    }

    /// <summary>
    /// Node representing a constant number literal
    /// </summary>
    public class ConstantNode : ValueNode
    {
        public int Value { get; set; }
        
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