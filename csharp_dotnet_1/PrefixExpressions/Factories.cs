using System;
using System.Collections.Generic;

namespace PrefixExpressions
{
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

    public class FormulaCreator : INodeParser
    {
        private Dictionary<string, IOpNodeFactory> opFactories = new Dictionary<string, IOpNodeFactory>();
        public IValueNodeFactory ConstantFactory { get; set; } = new ConstNodeFactory();
        private int i;

        public void AddFactory(string operatorToken, IOpNodeFactory factory)
        {
            opFactories[operatorToken] = factory;
        }
        
        public INode ParseFormula(string[] arguments)
        {
            i = 0;
            var result = RecursiveParse(arguments);
            if (i < arguments.Length - 1)
            {
                throw new FormatException($"Too many arguments on input.");
            }

            return result;
        }

        private INode RecursiveParse(string[] arguments)
        {
            if (i >= arguments.Length) throw new FormatException("Too few arguments on input.");
            

            if (!opFactories.TryGetValue(arguments[i], out IOpNodeFactory factory))
            {
                return ConstantFactory.CreateNew(arguments[i]);
            }

            OperatorNode result = factory.CreateNew();
            INode son;
            do
            {
                i++;
                son = RecursiveParse(arguments);
            } while (!result.AddSon(son));

            return result;
        }
    }
}