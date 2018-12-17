using System;
using System.Collections.Generic;

namespace PrefixExpressions
{
    #region factories
    // Factory for each node type
    public interface IValueNodeFactory
    {
        ValueNode CreateNew(string argument);
    }

    public interface IOpNodeFactory
    {
        OperatorNode CreateNew();
    }
    
    public class ConstNodeFactory : IValueNodeFactory
    {
        public ValueNode CreateNew(string argument)
        {
            bool ok = int.TryParse(argument, out int result);
            if (ok) return new ConstantNode{Value = result};
            
            throw new FormatException("Cannot parse integer.");
        }
    }

    public class PlusOpNodeFactory : IOpNodeFactory
    {
        public OperatorNode CreateNew()
        {
            return new PlusOpNode();
        }
    }
    
    public class MinusOpNodeFactory : IOpNodeFactory
    {
        public OperatorNode CreateNew()
        {
            return new MinusOpNode();
        }
    }
    
    public class MultiplyOpNodeFactory : IOpNodeFactory
    {
        public OperatorNode CreateNew()
        {
            return new MultiplyOpNode();
        }
    }
    
    public class DivideOpNodeFactory : IOpNodeFactory
    {
        public OperatorNode CreateNew()
        {
            return new DivideOpNode();
        }
    }
    
    public class NegateOpNodeFactory : IOpNodeFactory
    {
        public OperatorNode CreateNew()
        {
            return new NegateOpNode();
        }
    }
    #endregion
    
}