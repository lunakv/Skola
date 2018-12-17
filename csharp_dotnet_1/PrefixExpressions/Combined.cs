using System;
using System.Collections.Generic;
using System.IO;

namespace PrefixExpressions
{
    class Program
    {
        static void Main()
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
        int IntEvaluate(IntAlgorithm alg);
        double DoubleEvaluate(IDoubleAlgorithm alg);
    }

    public interface IntAlgorithm
    {
        int Call(PlusOpNode caller);
        int Call(MinusOpNode caller);
        int Call(MultiplyOpNode caller);
        int Call(DivideOpNode caller);
        int Call(NegateOpNode caller);
        int Call(ConstantNode caller);
    }

    public interface IDoubleAlgorithm
    {
        double Call(PlusOpNode caller);
        double Call(MinusOpNode caller);
        double Call(MultiplyOpNode caller);
        double Call(DivideOpNode caller);
        double Call(NegateOpNode caller);
        double Call(ConstantNode caller);
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
        
        public abstract int IntEvaluate(IntAlgorithm alg);
        public abstract double DoubleEvaluate(IDoubleAlgorithm alg);
        public abstract bool AddSon(INode son);
    }

    /// <summary>
    /// Main algorithm of the program. 
    /// </summary>
    public class PrefixCalculator
    {
        public INodeParser Parser { get; set; }                            // parser used for making new formulas
        private IntAlgorithm _intEvaler = new IntEvaluator();              // algorithm for integer evaluation
        private IDoubleAlgorithm _doubleEvaler = new DoubleEvaluator();    // algorithm for double evaluation
        private INode _formula;                                            // last processed formula
        
        /// <summary>
        /// Main processing function. Reads through the input and computes all the compatible operations.
        /// </summary>
        /// <param name="input">TextReader containing instructions for the calculator.</param>
        public void ProcessInput(TextReader input)
        {
            string line = input.ReadLine();
            while (line != null && line != "end")
            {
                try
                {
                    ParseLine(line);
                }
                catch (Exception e) when (e is FormatException || e is DivideByZeroException || e is OverflowException ||
                                          e is MissingMemberException)
                // Any input or evaluation errors are thrown as exceptions and will propagate to this point.
                {
                    DumpError(e);
                }
                
                line = input.ReadLine();
            }
        }
        
        /// <summary>
        /// Jump table separating different lines by content. 
        /// </summary>
        /// <param name="inputLine">single line read from input</param>
        /// <exception cref="FormatException"><paramref name="inputLine"/> has incorrect format (see Calculator summary)</exception>
        private void ParseLine(string inputLine)
        {
            if (inputLine == "")
            {
                return;
            }
            if (inputLine == "i")
            {
                EvalAsInt();
            }
            else if (inputLine == "d")
            {
                EvalAsDouble();
            }
            else if (inputLine[0] == '=')
            {
                ParseNewFormula(inputLine);
            }
            else
            {
                throw new FormatException("Input format not recognized.");
            }
        }
        
        /// <summary>
        /// Evaluates the buffered formula in integer form and writes the result to <code>Console.Out</code>
        /// </summary>
        /// <exception cref="MissingMemberException">No valid formula is currently buffered.</exception>
        /// <exception cref="FormatException">Propagated exception from evaluation.</exception>
        /// <exception cref="DivideByZeroException">Propagated exception from evaluation.</exception>
        /// <exception cref="OverflowException">Propagated exception from evaluation.</exception>
        private void EvalAsInt()
        {
            if (_formula == null)
            {
                throw new MissingMemberException("Trying to evaluate formula, but none is specified.");
            }

            int result = _formula.IntEvaluate(_intEvaler);
            Console.WriteLine(result);
        }

        /// <summary>
        /// Evaluates the buffered formula as a floating point number and writes the result to <code>Console.Out</code>
        /// </summary>
        /// <exception cref="MissingMemberException">No valid formula is currently buffered.</exception>
        /// <exception cref="FormatException">Propagated exception from evaluation.</exception>
        private void EvalAsDouble()
        {
            if (_formula == null)
            {
                throw new MissingMemberException("Trying to evaluate formula, but none is specified.");
            }

            double result = _formula.DoubleEvaluate(_doubleEvaler);
            Console.WriteLine(result.ToString("f05"));
        }

        /// <summary>
        /// Clears the formula buffer and tries to parse a new formula
        /// </summary>
        /// <param name="line">Formula to be parsed. Assumes <c>line[0] == '='</c></param>
        /// <exception cref="FormatException">empty formula, wrong number of tokens</exception>
        private void ParseNewFormula(string line)
        {
            _formula = null;     
            var splitLine = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (splitLine.Length == 1) throw new FormatException("Formula without any argument encountered");
            
            string[] arguments = new string[splitLine.Length-1];
            Array.Copy(splitLine, 1, arguments, 0, splitLine.Length-1);
            _formula = Parser.ParseFormula(arguments);
        }

        /// <summary>
        /// Writes an appropriate error message to Console.Out
        /// </summary>
        /// <param name="e">Exception to notify about</param>
        /// <exception cref="NotImplementedException"><paramref name="e"/> not a supported type.</exception>
        private static void DumpError(Exception e)
        {
            string errorMessage;
            switch (e)
            {
                case FormatException _:
                    errorMessage = "Format Error";
                    break;
                case DivideByZeroException _:
                    errorMessage = "Divide Error";
                    break;
                case OverflowException _:
                    errorMessage = "Overflow Error";
                    break;
                case MissingMemberException _:
                    errorMessage = "Expression Missing";
                    break;
                default:
                    throw new NotImplementedException("Unhandled error type passed.");
            }
            
            Console.WriteLine(errorMessage);
        }
    }

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

    /// <summary>
    /// Creates a tree of tokens specified in prefix notation
    /// </summary>
    public class PrefixFormulaCreator : INodeParser
    {
        private Dictionary<string, IOpNodeFactory> opFactories = new Dictionary<string, IOpNodeFactory>();
            // holds information about all specified operator factories
        public IValueNodeFactory ConstantFactory { get; } = new ConstNodeFactory();    // factory to create constants
        private int i;                                                                 // index of the processed token

        /// <summary>
        /// Adds a new operator by registering a <c>IOpNodeFactory</c> associated with that operator
        /// </summary>
        /// <param name="operatorToken">string representing the new operator</param>
        /// <param name="factory">factory creating objects associated with the operator</param>
        public void AddFactory(string operatorToken, IOpNodeFactory factory)
        {
            opFactories[operatorToken] = factory;
        }
        
        /// <summary>
        /// Creates a formula tree from an array of tokens.
        /// </summary>
        /// <param name="arguments">array of tokens</param>
        /// <returns>root of the tree, if the parse was successful</returns>
        /// <exception cref="FormatException">wrong number of tokens - either some are remaining, or an operator is missing a child</exception>
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

        /// <summary>
        /// Recursively goes through the token array to create the tree.
        /// </summary>
        /// <exception cref="FormatException">index exceeded length of the array - too few arguments on input</exception>
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

    /// <summary>
    /// Visitor used to evaluate nodes as integers
    /// </summary>
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
    
    /// <summary>
    /// Visitor used to evaluate nodes as floating point numbers
    /// </summary>
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