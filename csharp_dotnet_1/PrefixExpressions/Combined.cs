using System;
using System.Collections.Generic;
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

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class MinusOpNode : BinaryOpNode
    {
        public override char Operator => '-';

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class MultiplyOpNode : BinaryOpNode
    {
        public override char Operator => '*';

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class DivideOpNode : BinaryOpNode
    {
        public override char Operator => '/';

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
    
    public abstract class ValueNode : INode
    {
        public abstract T Accept<T>(IVisitor<T> visitor);
    }

    /// <summary>
    /// Node representing a constant number literal
    /// </summary>
    public class ConstantNode : ValueNode
    {
        public int Value { get; set; }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
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
    
    /// <summary>
    /// Writes formulas in infix notation fully parenthesized. Always returns true.
    /// </summary>
    public class FullInfixWriter : IVisitor<bool>
    {
        public bool Visit(PlusOpNode visited)
        {
            Console.Write("(");
            visited.LeftSon.Accept(this);
            Console.Write("+");
            visited.RightSon.Accept(this);
            Console.Write(")");
            return true;
        }

        public bool Visit(MinusOpNode visited)
        {
            Console.Write("(");
            visited.LeftSon.Accept(this);
            Console.Write("-");
            visited.RightSon.Accept(this);
            Console.Write(")");
            return true;

        }

        public bool Visit(MultiplyOpNode visited)
        {
            Console.Write("(");
            visited.LeftSon.Accept(this);
            Console.Write("*");
            visited.RightSon.Accept(this);
            Console.Write(")");
            return true;

        }

        public bool Visit(DivideOpNode visited)
        {
            Console.Write("(");
            visited.LeftSon.Accept(this);
            Console.Write("/");
            visited.RightSon.Accept(this);
            Console.Write(")");
            return true;
        }

        public bool Visit(NegateOpNode visited)
        {
            Console.Write("(");
            Console.Write("-");
            visited.Son.Accept(this);
            Console.Write(")");
            return true;
        }

        public bool Visit(ConstantNode visited)
        {
            Console.Write(visited.Value);
            return true;
        }
    }

    /// <summary>
    /// Writes formulas in infix notation with minimum number of parentheses. Always returns true.
    /// </summary>
    public class SimpleInfixWriter : IVisitor<bool>
    {
        readonly IVisitor<int> priorityGetter = new OpPriorityFinder();
        
        public bool Visit(PlusOpNode visited)
        {
            // + has lowest priority and is associative - no need for parentheses around either child
            visited.LeftSon.Accept(this); 
            Console.Write("+");
            visited.RightSon.Accept(this);
            return true;
        }

        public bool Visit(MinusOpNode visited)
        {
            // - has lowest priority - no need for parentheses around left child
            visited.LeftSon.Accept(this);
            Console.Write("-");
            
            // - is not associative - parentheses needed if right child has same priority
            int thisPriority = visited.Accept(priorityGetter);
            int rightSonPriority = visited.RightSon.Accept(priorityGetter);
            
            if (rightSonPriority <= thisPriority) Console.Write("(");
            visited.RightSon.Accept(this);
            if (rightSonPriority <= thisPriority) Console.Write(")");

            return true;
        }

        public bool Visit(MultiplyOpNode visited)
        {
            // parentheses might be needed around children with lower priorities
            int thisPriority = visited.Accept(priorityGetter);
            int sonPriority = visited.LeftSon.Accept(priorityGetter);
            
            if (sonPriority < thisPriority) Console.Write("(");
            visited.LeftSon.Accept(this);
            if (sonPriority < thisPriority) Console.Write(")");
            
            Console.Write("*");
            sonPriority = visited.RightSon.Accept(priorityGetter);
            
            // * is associative - no need for parentheses when RightSon has same priority
            if (sonPriority < thisPriority) Console.Write("(");
            visited.RightSon.Accept(this);
            if (sonPriority < thisPriority) Console.Write(")");

            return true;
        }

        public bool Visit(DivideOpNode visited)
        {
            // parentheses might be needed around children with lower priorities
            int thisPriority = visited.Accept(priorityGetter);
            int sonPriority = visited.LeftSon.Accept(priorityGetter);
            
            if (sonPriority < thisPriority) Console.Write("(");
            visited.LeftSon.Accept(this);
            if (sonPriority < thisPriority) Console.Write(")");
            
            Console.Write("/");
            sonPriority = visited.RightSon.Accept(priorityGetter);
            
            // "/" is not associative - same priority right sons need parentheses
            if (sonPriority <= thisPriority) Console.Write("(");
            visited.RightSon.Accept(this);
            if (sonPriority <= thisPriority) Console.Write(")");

            return true;
        }

        public bool Visit(NegateOpNode visited)
        {
            Console.Write("-");

            int thisPriority = visited.Accept(priorityGetter);
            int sonPriority = visited.Son.Accept(priorityGetter);
            
            //everything except value nodes needs parentheses
            if (sonPriority < thisPriority) Console.Write("(");
            visited.Son.Accept(this);
            if (sonPriority < thisPriority) Console.Write(")");

            return true;
        }

        public bool Visit(ConstantNode visited)
        {
            Console.Write(visited.Value);
            return true;
        }
    }

    /// <summary>
    /// Gets the priority of nodes in a formula.
    /// </summary>
    public class OpPriorityFinder : IVisitor<int>
    {
        public int Visit(PlusOpNode visited)
        {
            return 0;
        }

        public int Visit(MinusOpNode visited)
        {
            return 0;
        }

        public int Visit(MultiplyOpNode visited)
        {
            return 5;
        }

        public int Visit(DivideOpNode visited)
        {
            return 5;
        }

        public int Visit(NegateOpNode visited)
        {
            return 10;
        }

        public int Visit(ConstantNode visited)
        {
            return 100;    // high value guarantees no parentheses around constants
        }
    }
    
    /// <summary>
    /// Main algorithm of the program. 
    /// </summary>
    public class PrefixCalculator
    {
        public INodeParser Parser { get; set; }                                    // parser used for making new formulas
        private readonly IVisitor<int> _intEvaler = new IntEvaluator();            // algorithm for integer evaluation
        private readonly IVisitor<double> _doubleEvaler = new DoubleEvaluator();   // algorithm for double evaluation
        private readonly IVisitor<bool> _fullInfixWriter = new FullInfixWriter();  // full parentheses infix writer
        private readonly IVisitor<bool> _minInfixWriter = new SimpleInfixWriter(); // minimal parentheses infix writer
        private INode _formula;                                                    // last processed formula
        
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
                EvalAsInt(_formula);
            }
            else if (inputLine == "d")
            {
                EvalAsDouble(_formula);
            }
            else if (inputLine == "p")
            {
                PrintFullInfix(_formula);
            }
            else if (inputLine == "P")
            {
                PrintSimpleInfix(_formula);
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
        private void EvalAsInt(INode formula)
        {
            if (formula == null)
            {
                throw new MissingMemberException("Trying to evaluate formula, but none is specified.");
            }

            int result = formula.Accept(_intEvaler);
            Console.WriteLine(result);
        }

        /// <summary>
        /// Evaluates the buffered formula as a floating point number and writes the result to <code>Console.Out</code>
        /// </summary>
        /// <exception cref="MissingMemberException">No valid formula is currently buffered.</exception>
        /// <exception cref="FormatException">Propagated exception from evaluation.</exception>
        private void EvalAsDouble(INode formula)
        {
            if (formula == null)
            {
                throw new MissingMemberException("Trying to evaluate formula, but none is specified.");
            }

            double result = formula.Accept(_doubleEvaler);
            Console.WriteLine(result.ToString("f05"));
        }

        /// <summary>
        /// Writes the buffered formula in infix notation with all parentheses.
        /// </summary>
        /// <exception cref="MissingMemberException">No valid formula is currently buffered.</exception>
        private void PrintFullInfix(INode formula)
        {
            if (formula == null)
            {
                throw new MissingMemberException("No valid formula specified");
            }
            formula.Accept(_fullInfixWriter);
            Console.WriteLine();
        }

        /// <summary>
        /// Writes the buffered formula in infix notation, minimizing the number of parentheses written 
        /// </summary>
        /// <exception cref="MissingMemberException">No valid formula is currently buffered.</exception>
        private void PrintSimpleInfix(INode formula)
        {
            if (formula == null)
            {
                throw new MissingMemberException("No valid formula specified");
            }

            formula.Accept(_minInfixWriter);
            Console.WriteLine();
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
}