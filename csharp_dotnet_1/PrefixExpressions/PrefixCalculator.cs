using System;
using System.IO;

namespace PrefixExpressions
{
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
        private int bufferedIntValue;
        private double bufferedDoubleValue;
        private bool isIntBuffered, isDoubleBuffered;
        
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
            if (isIntBuffered)
            {
                Console.WriteLine(bufferedIntValue);
                return;
            }
            
            if (formula == null)
            {
                throw new MissingMemberException("Trying to evaluate formula, but none is specified.");
            }

            int result = formula.Accept(_intEvaler);
            bufferedIntValue = result;
            isIntBuffered = true;
            Console.WriteLine(bufferedIntValue);
        }

        /// <summary>
        /// Evaluates the buffered formula as a floating point number and writes the result to <code>Console.Out</code>
        /// </summary>
        /// <exception cref="MissingMemberException">No valid formula is currently buffered.</exception>
        /// <exception cref="FormatException">Propagated exception from evaluation.</exception>
        private void EvalAsDouble(INode formula)
        {
            if (isDoubleBuffered)
            {
                Console.WriteLine(bufferedDoubleValue.ToString("f05"));
                return;
            }
            
            if (formula == null)
            {
                throw new MissingMemberException("Trying to evaluate formula, but none is specified.");
            }

            double result = formula.Accept(_doubleEvaler);
            bufferedDoubleValue = result;
            isDoubleBuffered = true;
            Console.WriteLine(bufferedDoubleValue.ToString("f05"));
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
            isIntBuffered = false;
            isDoubleBuffered = false;
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