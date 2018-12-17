using System.Security.Cryptography;

namespace PrefixExpressions
{
    public abstract class BinaryOpNode : OperatorNode
    {
        public INode LeftSon { get; set; }
        public INode RightSon { get; set; }
        public override int Arity => 2;

        public override bool AddSon(INode son)
        {
            if (LeftSon == null)
            {
                LeftSon = son;
                return false;
            }

            RightSon = RightSon ?? son;
            return true;
        }
    }

    public class PlusOpNode : BinaryOpNode
    {
        public override char Operator => '+';
        
        public override int IntEvaluate(IntAlgorithm alg)
        {
            return alg.Call(this);
        }

        public override double DoubleEvaluate(IDoubleAlgorithm alg)
        {
            return alg.Call(this);
        }
    }

    public class MinusOpNode : BinaryOpNode
    {
        public override char Operator => '-';
        
        public override int IntEvaluate(IntAlgorithm alg)
        {
            return alg.Call(this);
        }

        public override double DoubleEvaluate(IDoubleAlgorithm alg)
        {
            return alg.Call(this);
        }

    }

    public class MultiplyOpNode : BinaryOpNode
    {
        public override char Operator => '*';
        
        public override int IntEvaluate(IntAlgorithm alg)
        {
            return alg.Call(this);
        }

        public override double DoubleEvaluate(IDoubleAlgorithm alg)
        {
            return alg.Call(this);
        }
    }

    public class DivideOpNode : BinaryOpNode
    {
        public override char Operator => '/';
        
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