using System;

namespace PrefixFormulas
{
	public enum ErrorState {Divide, Format, Overflow}
	
	class Program
	{
		static void Main(string[] args)
		{
			string[] arguments = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			IToken formula = new FormulaFactory().CreateFormula(arguments);
			IToken evald = formula.Evaluate();
			Console.Write(evald.ToString());	// overridden ToString() method
		}
	}

	public class FormulaFactory
	{
		private int index;				
		private ITokenFactory valueFactory = new ValueTokenFactory();
		private ITokenFactory opFactory = new OperatorTokenFactory();
		
		public IToken CreateFormula(string[] operands)
		{
			index = 0;
			IToken root = RecursiveCreateFormula(operands);
			return index == operands.Length ? root : new ErrorToken(ErrorState.Format); // check for too many tokens
		}
		
		// Recursively reads the input array and creates a tree of tokens
		private IToken RecursiveCreateFormula(string[] operands)
		{
			if (index >= operands.Length) return new ErrorToken(ErrorState.Format);	// required son of operator unavailable
			
			// creating the appropriate token
			IToken formulaToken;
			if ((formulaToken = valueFactory.CreateNew(operands[index])) == null)
			if ((formulaToken = opFactory.CreateNew(operands[index])) == null)
			formulaToken = new ErrorToken(ErrorState.Format);
			index++;
			
			// creating child tokens for operators
			if (formulaToken is OperatorToken opToken)
			{
				for (int i = 0; i < opToken.Operands.Length; i++)
				{
					opToken.Operands[i] = RecursiveCreateFormula(operands);
				}
			}

			return formulaToken;
		}
	}
	
	/// <summary>
	/// Token representing an invalid value. 
	/// </summary>
	public class ErrorToken : IToken
	{
		private ErrorState State { get; }

		public ErrorToken(ErrorState state)
		{
			State = state;
		}
		
		public IToken Evaluate()
		{
			return this;
		}

		public override string ToString()
		{
			return State.ToString() + " Error";
		}
	}

	/// <summary>
	/// Token representing a unary operator. 
	/// </summary>
	public class UnaryOperatorToken : OperatorToken
	{
		public UnaryOperatorToken(char op)
		{
			Operands = new IToken[1];
			Operator = op;
		}

		public override IToken Evaluate()
		{
			IToken sonEvald = Operands[0].Evaluate();
			if (!(sonEvald is ValueToken sonValue))
			{
				return sonEvald;
			}

			switch (Operator)
			{
				case '~':
					if (sonValue.Value == int.MinValue)
						return new ErrorToken(ErrorState.Overflow);
					return new ValueToken(- sonValue.Value);
				
				default:
					throw new NotImplementedException("Specified operator not implemented."); 
			}
		}
	}
	
	/// <summary>
	/// Token representing a binary operator.
	/// </summary>
	public class BinaryOperatorToken : OperatorToken
	{
		public BinaryOperatorToken(char op)
		{
			Operands = new IToken[2];
			Operator = op;
		}
		
		public override IToken Evaluate()
		{
			IToken leftEvaluated = Operands[0]?.Evaluate();
			if (!(leftEvaluated is ValueToken))
			{
				return leftEvaluated;
			}
			
			IToken rightEvaluated = Operands[1]?.Evaluate();
			if (!(rightEvaluated is ValueToken))
			{
				return rightEvaluated;
			}

			int leftValue = ((ValueToken) leftEvaluated).Value;
			int rightValue = ((ValueToken) rightEvaluated).Value;
			int result;

			try
			{
				switch (Operator)
				{
					case '+':
						result = checked(leftValue + rightValue);
						break;
					case '-':
						result = checked(leftValue - rightValue);
						break;
					case '*':
						result = checked(leftValue * rightValue);
						break;
					case '/':
						if (rightValue == 0)
						{
							return new ErrorToken(ErrorState.Divide);
						}
						else
						{
							result = leftValue / rightValue;
						}
						break;
					default:
						throw new NotImplementedException("Specified operator not implemented.");
				}
			}
			catch (OverflowException)
			{
				return new ErrorToken(ErrorState.Overflow);
			}
			
			return new ValueToken(result);
		}
	}

	/// <summary>
	/// Abstract class of all operator tokens
	/// </summary>
	public abstract class OperatorToken : IToken
	{
		protected char Operator { get; set; }
		public IToken[] Operands { get; protected set; }
		
		/// <summary>
		/// Evaluates the subformula starting with this token 
		/// </summary>
		/// <returns> A value token if evaluation is successful; an appropriate error token otherwise. </returns>
		public abstract IToken Evaluate();
	}

	/// <summary>
	/// Factory creating operator tokens.
	/// </summary>
	public class OperatorTokenFactory : ITokenFactory
	{
		private static readonly string[][] operators =
		{
			new string[0],
			new[] {"~"},
			new[] {"+", "-", "*", "/"}
		};
		
		public IToken CreateNew(string argument)
		{
			int arity = GetArity(argument);
			if (arity == -1) return null;
			var op = char.Parse(argument);
			
			switch (arity)
			{
				case 1:
					return new UnaryOperatorToken(op);
				case 2:
					return new BinaryOperatorToken(op);
				default:
					throw new NotImplementedException("Invalid number of operands");
			}
		}

		private int GetArity(string argument)
		{
			for (int i = 0; i < operators.Length; i++)
			{
				if (HasArity(argument, i)) return i;
			}

			return -1;
		}

		private bool HasArity(string argument, int arity)
		{
			if (arity < 0 || arity >= operators.Length) return false;

			foreach (string op in operators[arity])
			{
				if (op == argument) return true;
			}

			return false;
		}
	}

	public class ValueTokenFactory : ITokenFactory
	{
		public IToken CreateNew(string argument)
		{
			bool good = int.TryParse(argument, out int result);
			if (good)
			{
				return new ValueToken(result);
			}

			return null;
		}
	}
	
	/// <summary>
	/// Token representing a literal value
	/// </summary>
	public class ValueToken : IToken 
	{
		public int Value { get; }

		public ValueToken(int value)
		{
			Value = value;
		}
		
		public IToken Evaluate()
		{
			return this;
		}

		public static bool TryCreate(string argument, out IToken result)
		{
			if (int.TryParse(argument, out int value))
			{
				result = new ValueToken(value);
				return true;
			}

			result = null;
			return false;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public interface IToken
	{
		IToken Evaluate();
	}

	public interface ITokenFactory
	{
		IToken CreateNew(string argument);
	}
}