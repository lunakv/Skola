using System;
using System.Collections.Generic;

namespace PrefixExpressions
{
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
}