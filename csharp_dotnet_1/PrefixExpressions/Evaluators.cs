namespace PrefixExpressions
{
    /// <summary>
    /// Visitor used to evaluate nodes as integers
    /// </summary>
    public class IntEvaluator : IVisitor<int>
    {
        public int Visit(PlusOpNode caller)
        {
            int left = caller.LeftSon.Accept(this);
            int right = caller.RightSon.Accept(this);
            return checked(left + right);
        }

        public int Visit(MinusOpNode caller)
        {
            int left = caller.LeftSon.Accept(this);
            int right = caller.RightSon.Accept(this);
            return checked(left - right);
        }

        public int Visit(MultiplyOpNode caller)
        {
            int left = caller.LeftSon.Accept(this);
            int right = caller.RightSon.Accept(this);
            return checked(left * right);
        }

        public int Visit(DivideOpNode caller)
        {
            int left = caller.LeftSon.Accept(this);
            int right = caller.RightSon.Accept(this);
            return left / right;
        }

        public int Visit(NegateOpNode caller)
        {
            int son = caller.Son.Accept(this);
            return checked(-son);
        }

        public int Visit(ConstantNode caller)
        {
            return caller.Value;
        }
    }
    
    /// <summary>
    /// Visitor used to evaluate nodes as floating point numbers
    /// </summary>
    public class DoubleEvaluator : IVisitor<double>
    {
        public double Visit(PlusOpNode caller)
        {
            double left = caller.LeftSon.Accept(this);
            double right = caller.RightSon.Accept(this);
            return left + right;        
        }

        public double Visit(MinusOpNode caller)
        {
            double left = caller.LeftSon.Accept(this);
            double right = caller.RightSon.Accept(this);
            return left - right;
        }

        public double Visit(MultiplyOpNode caller)
        {
            double left = caller.LeftSon.Accept(this);
            double right = caller.RightSon.Accept(this);
            return left * right;        
        }

        public double Visit(DivideOpNode caller)
        {
            double left = caller.LeftSon.Accept(this);
            double right = caller.RightSon.Accept(this);
            return left / right;        
        }

        public double Visit(NegateOpNode caller)
        {
            double son = caller.Son.Accept(this);
            return -son;
        }

        public double Visit(ConstantNode caller)
        {
            return caller.Value;
        }
    }
}