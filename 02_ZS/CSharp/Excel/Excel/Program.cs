using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Excel_tests")]
namespace Excel
{
	/// <summary>
	/// Describes whether the cell has a correct value or contains some kind of error.
	/// </summary>
	public enum Status
	{
		CORRECT,
		INVVAL,
		ERROR,
		DIV0,
		CYCLE,
		FORMULA,
		MISSOP
	}

	/// <summary>
	/// Describes the stage of the evaluation process of a cell. 
	/// </summary>
	public enum Evaluation
	{
		Evaluated,
		Evaluating, 
		NotEvaluated
	}

	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args?.Length != 2)
			{
				ThrowArgumentError();
				return;
			}

			TextReader input;
			TextWriter output;

			try
			{
				input = new StreamReader(args[0]);
				output = new StreamWriter(args[1]);
			}
			catch (Exception e) when (e is ArgumentException || e is FileNotFoundException ||
			                          e is DirectoryNotFoundException || e is IOException)
			{
				ThrowFileError();
				return;
			}
			
			var eval = new Excel();
			eval.Evaluate(input, output);
			input.Dispose();
			output.Dispose();
		}

		static void ThrowArgumentError()
		{
			Console.Write("Argument Error");
		}

		static void ThrowFileError()
		{
			Console.Write("File Error");
		}
	}

	/// <summary>
	/// Key class representing one cell of a sheet.
	/// </summary>
	internal class Cell
	{
		public Status Status { get; set; }				// see Status enum summary
		public Position Coord { get; private set; }		// position of the cell in a sheet, indexed from one in both axes. 
		public Evaluation Evaluation { get; set; } = Evaluation.NotEvaluated;	// see Evaluation enum summary
		public int Value { get; set; }					// numerical value of cell. only defined if Status == CORRECT
		public Position[] DependentOn { get; private set; } = new Position[0]; 	// all positions cell's value depend on
		public char Operator { get; private set; }		// operator in cell's formula. can be '+', '-', '*', or '/'

		private Cell() { } // private constructor. use GenerateNew to create cells.

		/// <summary>
		/// Creates a new cell based on the specified parameters.
		/// </summary>
		/// <param name="argument"> String representation of the cell read from input. </param>
		/// <param name="row"> Row the cell should be put on, indexed from one. </param>
		/// <param name="column"> Column the cell should be put on, indexed from one. </param>
		/// <returns> A new Cell corresponding to the specified values. </returns>
		public static Cell GenerateNew(string argument, int row, int column)
		{
			if (argument == "[]") return null; // no need to store empty cells;
			if (argument[0] != '=')			   // argument is not a formula
			{
				bool correct = int.TryParse(argument, out int result);
				if (correct)
					return new Cell {Coord = new Position(row, column), Value = result};
				
				return new Cell {Status = Status.INVVAL};
			}

			// creating a cell from a parsed formula.
			Status stat = ArgumentParser.ProcessArgument(argument, out Position[] dependent, out char op);
			return new Cell
			{
				Operator = op, 
				Status = stat, 
				Coord = new Position(row, column), 
				DependentOn = dependent
			};
		}
	}

	/// <summary>
	/// Static class used to parse sheet cell formulas. 
	/// </summary>
	static class ArgumentParser
	{
		private static readonly char[] Operators = {'+', '-', '*', '/'}; // list of possible operators
		
		/// <summary>
		/// Processes the string argument and separates it into one operator and two operands.
		/// </summary>
		/// <param name="argument"> String argument representing a cell formula. </param>
		/// <param name="operands"> Array containing, in order, positions of both operands in <paramref name="argument"/>,
		/// 						or an empty array[0] if the formula is incorrect. </param>
		/// <param name="op"> The operator separating the two operands, or 0 if the formula is incorrect.</param>
		/// <returns> Status.MISSOP if formula contains no operator, Status.FORMULA if formula is otherwise incorrect,
		/// 		  Status.Correct otherwise. </returns>
		public static Status ProcessArgument(string argument, out Position[] operands, out char op)
		{
			operands = new Position[0]; // default values for out parameters
			op = (char) 0;
			
			string[] operandStrings = argument.Split('+', '-', '*', '/');
			if (operandStrings.Length == 1) return Status.MISSOP;
			if (operandStrings.Length != 2) return Status.FORMULA;

			if (ParseOperand(operandStrings[0], out Position firstOp) && ParseOperand(operandStrings[1], out Position secondOp))
			{
				operands = new[] {firstOp, secondOp};
				op = GetOperator(argument);
				return Status.CORRECT;
			}

			return Status.FORMULA;
		}

		/// <summary>
		/// Takes a string with an operand and converts it to a Position.
		/// </summary>
		/// <param name="operand"> String representation of a cell's position (may include '=' at the beginning). </param>
		/// <param name="opPosition"> Position represented by the <paramref name="operand"/>, or null if the operand is incorrect. </param>
		/// <returns> True if conversion succeeded, false if it failed. </returns>
		static bool ParseOperand(string operand, out Position opPosition)
		{
			opPosition = null;		// default value for out parameters
			if (string.IsNullOrEmpty(operand)) return false;
			
			int column = 0;
			int i = (operand[0] == '=') ? 1 : 0; // starting position to parse the operand

			while (i < operand.Length && operand[i] >= 'A' && operand[i] <= 'Z')
			{
				column = column * 26 + operand[i] - 'A' + 1;
				i++;
			}

			if (column == 0)	// no column characters were read -> wrong format
				return false;

			int row = 0;
			while (i < operand.Length && operand[i] >= '0' && operand[i] <= '9')
			{
				row = row * 10 + operand[i] - '0';
				i++;
			}

			if (row == 0)	// no numbers were read -> wrong format
				return false;
			if (i + 1 < operand.Length)		// characters remaining after last number -> wrong format
				return false;
			
			opPosition = new Position(row, column);
			return true;
		}

		/// <summary>
		/// Returns the first operator that appears in a string.
		/// </summary>
		/// <param name="argument"> String to be searched. </param>
		/// <returns> The first operator character found in <paramref name="argument"/>, or 0 if none are found. </returns>
		static char GetOperator(string argument)
		{
			for (int i = 0; i < argument.Length; i++)
			{
				if (IsOperand(argument[i]))
					return argument[i];
			}

			return (char) 0;
		}

		static bool IsOperand(char value)
		{
			foreach (char op in Operators)
			{
				if (value == op) return true;
			}

			return false;
		}
	}
	
	/// <summary>
	/// Represents the position of a cell in a sheet
	/// </summary>
	class Position
	{
		public int Row { get; }
		public int Column { get; }

		public Position(int row, int column)
		{
			Row = row;
			Column = column;
		}
		
		
	}

	/// <summary>
	/// Represents one row of cells in a sheet
	/// </summary>
	internal class Row
	{
		private List<Cell> _cells = new List<Cell>();	// list of all cells in the row (accessed by indexer)
		private int _index;								// index of the row in a sheet (indexed from one)

		public Cell this[int index] => index > _cells.Count ? null : _cells[index - 1]; // indexed from one

		public int Count => _cells.Count ; 				// number of cells in the row 
		
		public Row(int index)
		{
			_index = index;
		}
		
		/// <summary>
		/// Adds a new cell to this row.
		/// </summary>
		/// <param name="argument"> Argument (value or formula) representing the cell read from input. </param>
		public void AddCell(string argument)
		{
			var newCell = Cell.GenerateNew(argument, _index, _cells.Count + 1);
			_cells.Add(newCell);
		}
	}

	/// <summary>
	/// Represents one whole sheet of cells.
	/// </summary>
	internal class Sheet
	{
		private List<Row> _rows = new List<Row>(); 	// list of all rows in sheet, accessed by indexer
		public int RowCount => _rows.Count;			// number of rows in sheet

		public Row this[int index] => index > _rows.Count ? null : _rows[index - 1]; // indexed from one.

		/// <summary>
		/// Creates a new sheet based on specified input reader.
		/// </summary>
		/// <param name="argReader"> Reader to receive cell information from. </param>
		/// <returns> A new sheet based on the input read. </returns>
		public static Sheet GenerateSheet(IArgReader argReader)
		{
			Sheet result = new Sheet();
			result._rows.Add(new Row(1));
			string argument;
			
			while ((argument = argReader.ReadArgument()) != null)
			{
				if (argument == "\n")
				{
					var newRow = new Row(result._rows.Count + 1);
					result._rows.Add(newRow);
				}
				else
				{
					result._rows[result._rows.Count-1].AddCell(argument);
				}
			}

			return result;
		}
	}

	/// <summary>
	/// Algorithm for creating, evaluating and outputting sheets.
	/// </summary>
	class Excel
	{
		public void Evaluate(TextReader input, TextWriter output)
		{
			var reader = new SheetReader(input);
			var sheet = Sheet.GenerateSheet(reader);

			var evaluator = new SheetEvaluator(sheet);
			evaluator.EvaluateSheet();

			var writer = new SheetWriter(output);
			writer.WriteSheet(sheet);
		}
	}

	/// <summary>
	/// Used to evaluate all formulas in a given sheet.
	/// </summary>
	class SheetEvaluator
	{
		private Sheet _sheet;				// sheet to be evaluated
		private Stack<Cell> _cellStack;		// stack of cells, used to traverse dependencies
		
		public SheetEvaluator(Sheet sheet)
		{
			_sheet = sheet;
		}

		/// <summary>
		/// Substitutes correct values for all formulas in the sheet.
		/// </summary>
		public void EvaluateSheet()
		{
			_cellStack = new Stack<Cell>();
			
			for (int i = 1; i <= _sheet.RowCount; i++)
			{
				Row r = _sheet[i];
				for (int j = 1; j <= r.Count; j++)
				{
					Cell c = r[j];
					if (c?.Evaluation == Evaluation.NotEvaluated) // null cells are [] by design -> no need to evaluate
						SolveDependencies(c);
				}
			}
		}

		/// <summary>
		/// Searches through and evaluates all dependencies a cell has (including recursive ones), then evaluates the cell.
		/// </summary>
		/// <param name="cell"> Cell to be evaluated. </param>
		private void SolveDependencies(Cell cell)
		{
			_cellStack.Push(cell);
			
			while (_cellStack.Count != 0)		// traversing the dependency tree with a DFS search 
			{
				Cell currentCell = _cellStack.Peek();
				if (currentCell == null)			// should never happen
				{
					_cellStack.Pop();
					continue;
				}

				bool addedToStack = false;			
				foreach (var position in currentCell.DependentOn)	// looking for undiscovered dependencies
				{
					Cell dependency = _sheet[position.Row]?[position.Column];
					if (dependency == null) continue;				// [] is always independent and evaluated -> no need to push it
					
					if (dependency.Evaluation == Evaluation.NotEvaluated)	// new dependency found -> push it on the stack
																			// and stop searching neighbours
					{
						dependency.Evaluation = Evaluation.Evaluating;
						_cellStack.Push(dependency);
						addedToStack = true;
						break;
					}
					if (dependency.Evaluation == Evaluation.Evaluating)		// dependency still on stack -> cycle found
					{
						MarkCycle(dependency);
					}
				}

				if (addedToStack) continue; 	// we added some neighbours, so we can't pop current cell yet.

				EvaluateCell(_cellStack.Pop());
			}
		}

		/// <summary>
		/// Generates the value of a cell, assuming all dependencies are already evaluated.
		/// </summary>
		/// <param name="toEvaluate"> Cell which gains evaluation. </param>
		private void EvaluateCell(Cell toEvaluate)
		{
			toEvaluate.Evaluation = Evaluation.Evaluated;
			if (toEvaluate.Status != Status.CORRECT) return;	// wrong status -> don't care about value
			if (toEvaluate.DependentOn.Length < 2) return;		// correct status -> no dependencies

			Position pos1 = toEvaluate.DependentOn[0];
			Cell dependency1 = _sheet[pos1.Row]?[pos1.Column];
			Position pos2 = toEvaluate.DependentOn[1];
			Cell dependency2 = _sheet[pos2.Row]?[pos2.Column];

			if ((dependency1 != null && dependency1.Status != Status.CORRECT) ||	// >=1 dependency has incorrect status
			    (dependency2 != null && dependency2.Status != Status.CORRECT))
			{
				toEvaluate.Status = Status.ERROR;
				return;
			}

			int value1 = dependency1?.Value ?? 0;	// nulls are [] or unspecified cells, which have value 0
			int value2 = dependency2?.Value ?? 0;
			
			switch (toEvaluate.Operator)
			{
				case '+':
					toEvaluate.Value = value1 + value2;
					return;
				case '-':
					toEvaluate.Value = value1 - value2;
					return;
				case '*':
					toEvaluate.Value = value1 * value2;
					return;
				case '/':
					if (value2 == 0)
						toEvaluate.Status = Status.DIV0;
					else
						toEvaluate.Value = value1 / value2;
					return;
			}
		}

		/// <summary>
		/// Takes all members of a found cycle and labels them accordingly. 
		/// </summary>
		/// <param name="lastMember"> The last member of the cycle on the stack. </param>
		private void MarkCycle(Cell lastMember)
		{
			// there is no need to retain cycle members on the stack
			// we don't need any other dependencies to get their status, 
			// and all unevaluated cells dependent on them will be evaluated later in the main cycle of EvaluateSheet.
			
			Cell inCycleCell = _cellStack.Pop();
			while (inCycleCell != lastMember)
			{
				inCycleCell.Status = Status.CYCLE;
				inCycleCell.Evaluation = Evaluation.Evaluated;
				inCycleCell = _cellStack.Pop();
			}

			inCycleCell.Status = Status.CYCLE;				// last member of the cycle
			inCycleCell.Evaluation = Evaluation.Evaluated;
		}
	}
	
	/// <summary>
	/// Reader of arguments used to load a sheet.
	/// </summary>
	class SheetReader : IArgReader
	{
		private TextReader _input;
		private bool _readNewLine;
		private static readonly int[] _separators = {' ', '\t', '\n', -1};
		
		
		public SheetReader(TextReader input)
		{
			_input = input;
		}
		
		public int Read()
		{
			return _input.Read();
		}

		/// <summary>
		/// Reads one argument from specified input separated by whitespaces. Marks end of line.
		/// </summary>
		/// <returns> null if EOF reached, "\n" if a newline was encountered, otherwise one argument read from input stream. </returns>
		public string ReadArgument()
		{
			if (_readNewLine)
			{
				_readNewLine = false;
				return "\n";
			}

			int c = _input.Read();

			while (IsSeparator(c))				// skipping leading whitespace
			{
				if (c == '\n') return "\n";
				if (c == -1) return null;
				c = _input.Read();
			}
			
			StringBuilder result = new StringBuilder();

			while (!IsSeparator(c))
			{
				result.Append((char) c);
				c = _input.Read();
			}

			if (c == '\n') _readNewLine = true;
			return result.ToString();
		}

		private bool IsSeparator(int c)
		{
			foreach (int separator in _separators)
			{
				if (separator == c) return true;
			}

			return false;
		}
	}

	/// <summary>
	/// Writes the values of a sheet to the specified output.
	/// </summary>
	class SheetWriter
	{
		private TextWriter _output;
		
		public SheetWriter(TextWriter output)
		{
			_output = output;
		}

		public void WriteSheet(Sheet sheet)
		{
			for (int i = 1; i <= sheet.RowCount; i++)
			{
				Row row = sheet[i];
				
				for (int j = 1; j <= row.Count; j++)
				{
					Cell cell = row[j];
					if (cell == null)
						_output.Write("[]");
					else if (cell.Status == Status.CORRECT)
						_output.Write(cell.Value);
					else
						_output.Write("#" + cell.Status);

					_output.Write(j == row.Count ? "\n" : " ");
				}
			}
			
		}
	}
	
	public interface IArgReader
	{
		int Read();
		string ReadArgument();
	}
}