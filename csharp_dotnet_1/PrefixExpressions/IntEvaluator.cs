namespace PrefixExpressions
{
    public class IntEvaluator : IntAlgorithm
    {
        public int Call(PlusOpNode caller)
        {
            int left = caller.LeftSon.IntEvaluate(this);
            int right = caller.RightSon.IntEvaluate(this);
            return checked(left + right);
        }

        public int Call(MinusOpNode caller)
        {
            int left = caller.LeftSon.IntEvaluate(this);
            int right = caller.RightSon.IntEvaluate(this);
            return checked(left - right);
        }

        public int Call(MultiplyOpNode caller)
        {
            int left = caller.LeftSon.IntEvaluate(this);
            int right = caller.RightSon.IntEvaluate(this);
            return checked(left * right);
        }

        public int Call(DivideOpNode caller)
        {
            int left = caller.LeftSon.IntEvaluate(this);
            int right = caller.RightSon.IntEvaluate(this);
            return left / right;
        }

        public int Call(NegateOpNode caller)
        {
            int son = caller.Son.IntEvaluate(this);
            return checked(-son);
        }

        public int Call(ConstantNode caller)
        {
            return caller.Value;
        }
    }
    
    public class DoubleEvaluator : IDoubleAlgorithm
    {
        public double Call(PlusOpNode caller)
        {
            double left = caller.LeftSon.DoubleEvaluate(this);
            double right = caller.RightSon.DoubleEvaluate(this);
            return left + right;        
        }

        public double Call(MinusOpNode caller)
        {
            double left = caller.LeftSon.DoubleEvaluate(this);
            double right = caller.RightSon.DoubleEvaluate(this);
            return left - right;
        }

        public double Call(MultiplyOpNode caller)
        {
            double left = caller.LeftSon.DoubleEvaluate(this);
            double right = caller.RightSon.DoubleEvaluate(this);
            return left * right;        
        }

        public double Call(DivideOpNode caller)
        {
            double left = caller.LeftSon.DoubleEvaluate(this);
            double right = caller.RightSon.DoubleEvaluate(this);
            return left / right;        
        }

        public double Call(NegateOpNode caller)
        {
            double son = caller.Son.DoubleEvaluate(this);
            return -son;
        }

        public double Call(ConstantNode caller)
        {
            return caller.Value;
        }
    }
}