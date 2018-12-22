using System;
using System.IO;

namespace PrefixExpressions
{
    public static class Program
    {
        public static void Main()
        {
            // Creating the parser used for making new formulas
            var parser = new PrefixFormulaCreator();
            parser.AddFactory("+", new PlusOpNodeFactory());
            parser.AddFactory("-", new MinusOpNodeFactory());
            parser.AddFactory("*", new MultiplyOpNodeFactory());
            parser.AddFactory("/", new DivideOpNodeFactory());
            parser.AddFactory("~", new NegateOpNodeFactory());
            
            var calc = new PrefixCalculator{Parser = parser};
            calc.ProcessInput(Console.In);
        }
    }

    public interface INode
    {
        T Accept<T>(IVisitor<T> visitor);
    }


    public interface IVisitor<T>
    {
        T Visit(PlusOpNode visited);
        T Visit(MinusOpNode visited);
        T Visit(MultiplyOpNode visited);
        T Visit(DivideOpNode visited);
        T Visit(NegateOpNode visited);
        T Visit(ConstantNode visited);
    }

    public interface INodeParser
    {
        INode ParseFormula(string[] arguments);
        void AddFactory(string op, IOpNodeFactory factory);
    }
    
    public abstract class OperatorNode : INode
    {
        public abstract char Operator { get; }
        public abstract int Arity { get; }

        public abstract T Accept<T>(IVisitor<T> visitor);
        public abstract bool AddSon(INode son);
    }
}