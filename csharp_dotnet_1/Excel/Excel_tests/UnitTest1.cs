using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Excel;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;

namespace Excel_tests
{
	[TestClass]
	public class ArgumentParser_tests
	{
		/*[TestMethod]
		public void ProcessArgument_JustEqualSign()
		{
			Status res = ArgumentParser.ProcessArgument("=", out Position[] pos, out char op);
			
			Assert.AreEqual(Status.MISSOP, res);
			Assert.IsNull(pos);
			Assert.AreEqual((char) 0, op);
		}

		[TestMethod]
		public void ProcessArgument_JustOneArgument()
		{
			Status res = ArgumentParser.ProcessArgument("=ABC35", out Position[] pos, out char op);
			
			Assert.AreEqual(Status.MISSOP, res);
			Assert.IsNull(pos);
			Assert.AreEqual((char) 0, op);
		}

		[TestMethod]
		public void ProcessArgument_CorrectAddition()
		{
			Status res = ArgumentParser.ProcessArgument("=A11+BA12", out Position[] pos, out char op);
			var dependencies = new[] {new Position(11, 1), new Position(12, 53)};
			
			Assert.AreEqual(Status.CORRECT, res);
			Assert.AreEqual(dependencies[0].Row, pos[0].Row);
			Assert.AreEqual(dependencies[0].Column, pos[0].Column);
			Assert.AreEqual(dependencies[1].Row, pos[1].Row);
			Assert.AreEqual(dependencies[1].Column, pos[1].Column);
			Assert.AreEqual('+', op);
		}

		[TestMethod]
		public void ProcessArgument_CorrectSubtraction()
		{
			Status res = ArgumentParser.ProcessArgument("=CA32-BA12", out Position[] pos, out char op);
			var dependencies = new[] {new Position(32, 79), new Position(12, 53)};
			
			Assert.AreEqual(Status.CORRECT, res);
			Assert.AreEqual(dependencies[0].Row, pos[0].Row);
			Assert.AreEqual(dependencies[0].Column, pos[0].Column);
			Assert.AreEqual(dependencies[1].Row, pos[1].Row);
			Assert.AreEqual(dependencies[1].Column, pos[1].Column);
			Assert.AreEqual('-', op);
		}

		[TestMethod]
		public void ProcessArgument_CorrectMultiplication()
		{
			Status res = ArgumentParser.ProcessArgument("=A11*BAA123", out Position[] pos, out char op);
			var dependencies = new[] {new Position(11, 1), new Position(123, 1379)};
			
			Assert.AreEqual(Status.CORRECT, res);
			Assert.AreEqual(dependencies[0].Row, pos[0].Row);
			Assert.AreEqual(dependencies[0].Column, pos[0].Column);
			Assert.AreEqual(dependencies[1].Row, pos[1].Row);
			Assert.AreEqual(dependencies[1].Column, pos[1].Column);
			Assert.AreEqual('*', op);
		}

		[TestMethod]
		public void ProcessArgument_CorrectDivision()
		{
			Status res = ArgumentParser.ProcessArgument("=A11/BA12", out Position[] pos, out char op);
			var dependencies = new Position[] {new Position(11, 1), new Position(12, 53)};
			
			Assert.AreEqual(Status.CORRECT, res);
			Assert.AreEqual(dependencies[0].Row, pos[0].Row);
			Assert.AreEqual(dependencies[0].Column, pos[0].Column);
			Assert.AreEqual(dependencies[1].Row, pos[1].Row);
			Assert.AreEqual(dependencies[1].Column, pos[1].Column);
			Assert.AreEqual('/', op);
		}

		[TestMethod]
		public void ProcessArgument_LowerCaseOperand()
		{
			Status res = ArgumentParser.ProcessArgument("=ab11+BA12", out Position[] pos, out char op);
			
			Assert.AreEqual((char) 0, op);
			Assert.AreEqual(Status.FORMULA, res);
			Assert.IsNull(pos);
		}

		[TestMethod]
		public void ProcessArgument_LetterNumberLetterOperand()
		{
			Status res = ArgumentParser.ProcessArgument("=A11+BA12CD", out Position[] pos, out char op);
			
			Assert.AreEqual((char) 0, op);
			Assert.AreEqual(Status.FORMULA, res);
			Assert.IsNull(pos);
		}

		[TestMethod]
		public void ProcessArgument_MissingFirstOperand()
		{
			Status res = ArgumentParser.ProcessArgument("=+BA12", out Position[] pos, out char op);
			
			Assert.AreEqual((char) 0, op);
			Assert.AreEqual(Status.FORMULA, res);
			Assert.IsNull(pos);
		}
		
		[TestMethod]
		public void ProcessArgument_MissingSecondOperand()
		{
			Status res = ArgumentParser.ProcessArgument("=AB14*", out Position[] pos, out char op);
			
			Assert.AreEqual((char) 0, op);
			Assert.AreEqual(Status.FORMULA, res);
			Assert.IsNull(pos);
		}
		
		[TestMethod]
		public void ProcessArgument_WordInsteadOfOperand()
		{
			Status res = ArgumentParser.ProcessArgument("=AG13-testing", out Position[] pos, out char op);
			
			Assert.AreEqual((char) 0, op);
			Assert.AreEqual(Status.FORMULA, res);
			Assert.IsNull(pos);
		}
		
		[TestMethod]
		public void ProcessArgument_MissingColumnInOperand()
		{
			Status res = ArgumentParser.ProcessArgument("=AG13/16", out Position[] pos, out char op);
			
			Assert.AreEqual((char) 0, op);
			Assert.AreEqual(Status.FORMULA, res);
			Assert.IsNull(pos);
		}
		
		[TestMethod]
		public void ProcessArgument_MissingRowInOperand()
		{
			Status res = ArgumentParser.ProcessArgument("=AG-BA17", out Position[] pos, out char op);
			
			Assert.AreEqual((char) 0, op);
			Assert.AreEqual(Status.FORMULA, res);
			Assert.IsNull(pos);
		}
	}

	[TestClass]
	public class Cell_tests
	{
		[TestMethod]
		public void GenerateNew_JustEqualSign()
		{
			FormulaCell cell = FormulaCell.GenerateNew("=", 5, 7);
			
			Assert.IsNotNull(cell);
			Assert.AreEqual(Status.MISSOP, cell.Status);
		}

		[TestMethod]
		public void GenerateNew_StringAsValue()
		{
			FormulaCell cell = FormulaCell.GenerateNew("bananas", 5, 7);
			
			Assert.IsNotNull(cell);
			Assert.AreEqual(Status.INVVAL, cell.Status);	
		}
		
		[TestMethod]
		public void GenerateNew_PositiveLiteralValue()
		{
			FormulaCell cell = FormulaCell.GenerateNew("46", 16, 9);
			
			Assert.IsNotNull(cell);
			Assert.AreEqual(Status.CORRECT, cell.Status);
			Assert.AreEqual(46, cell.Value);
			Assert.AreEqual(16, cell.Coord.Row);
			Assert.AreEqual(9, cell.Coord.Column);
			Assert.IsNull(cell.DependentOn);
		}
		
		[TestMethod]
		public void GenerateNew_NegativeLiteralValue()
		{
			FormulaCell cell = FormulaCell.GenerateNew("-19", 2, 83);
			
			Assert.IsNotNull(cell);
			Assert.AreEqual(Status.CORRECT, cell.Status);
			Assert.AreEqual(-19, cell.Value);
			Assert.AreEqual(2, cell.Coord.Row);
			Assert.AreEqual(83, cell.Coord.Column);
			Assert.IsNull(cell.DependentOn);
		}
		
		[TestMethod]
		public void GenerateNew_OneOperandOnlyFormula()
		{
			FormulaCell cell = FormulaCell.GenerateNew("=AD14", 16, 9);
			
			Assert.IsNotNull(cell);
			Assert.AreEqual(Status.MISSOP, cell.Status);
		}
		
		[TestMethod]
		public void GenerateNew_OneOperandAndOperatorFormula()
		{
			FormulaCell cell = FormulaCell.GenerateNew("=AD14+", 16, 9);
			
			Assert.IsNotNull(cell);
			Assert.AreEqual(Status.FORMULA, cell.Status);
		}
		
		[TestMethod]
		public void GenerateNew_OperatorAndOneOperandFormula()
		{
			FormulaCell cell = FormulaCell.GenerateNew("=-AD14", 16, 9);
			
			Assert.IsNotNull(cell);
			Assert.AreEqual(Status.FORMULA, cell.Status);
		}
		
		[TestMethod]
		public void GenerateNew_BracketsValue()
		{
			FormulaCell cell = FormulaCell.GenerateNew("[]", 16, 9);
			
			Assert.IsNull(cell);
		}
		
		[TestMethod]
		public void GenerateNew_InvalidFirstOperandFormula()
		{
			FormulaCell cell = FormulaCell.GenerateNew("=Ad14+B36", 16, 9);
			
			Assert.IsNotNull(cell);
			Assert.AreEqual(Status.FORMULA, cell.Status);
		}
		
		[TestMethod]
		public void GenerateNew_InvalidSecondOperandFormula()
		{
			FormulaCell cell = FormulaCell.GenerateNew("=AD14/autobus", 16, 9);
			
			Assert.IsNotNull(cell);
			Assert.AreEqual(Status.FORMULA, cell.Status);
		}
		
		[TestMethod]
		public void GenerateNew_CorrectAdditionFormula()
		{
			FormulaCell cell = FormulaCell.GenerateNew("=AD14+B23", 16, 9);
			var deps = new[] {new Position(14, 30), new Position(23, 2)};
			
			Assert.IsNotNull(cell);
			Assert.AreEqual('+', cell.Operator);
			Assert.AreEqual(Status.CORRECT, cell.Status);
			Assert.AreEqual(16, cell.Coord.Row);
			Assert.AreEqual(9, cell.Coord.Column);
			for (int i = 0; i < 2; i++)
			{
				Assert.AreEqual(deps[i].Row, cell.DependentOn[i].Row);
				Assert.AreEqual(deps[i].Column, cell.DependentOn[i].Column);
			}
		}
		
		[TestMethod]
		public void GenerateNew_CorrectSubtractionFormula()
		{
			FormulaCell cell = FormulaCell.GenerateNew("=BC17-B23", 16, 9);
			var deps = new[] {new Position(17, 55), new Position(23, 2)};
			
			Assert.IsNotNull(cell);
			Assert.AreEqual('-', cell.Operator);
			Assert.AreEqual(Status.CORRECT, cell.Status);
			Assert.AreEqual(16, cell.Coord.Row);
			Assert.AreEqual(9, cell.Coord.Column);
			for (int i = 0; i < 2; i++)
			{
				Assert.AreEqual(deps[i].Row, cell.DependentOn[i].Row);
				Assert.AreEqual(deps[i].Column, cell.DependentOn[i].Column);
			}
		}
		
		[TestMethod]
		public void GenerateNew_CorrectMultiplicationFormula()
		{
			FormulaCell cell = FormulaCell.GenerateNew("=AD14*BA230", 16, 9);
			var deps = new[] {new Position(14, 30), new Position(230, 53)};
			
			Assert.IsNotNull(cell);
			Assert.AreEqual('*', cell.Operator);
			Assert.AreEqual(Status.CORRECT, cell.Status);
			Assert.AreEqual(16, cell.Coord.Row);
			Assert.AreEqual(9, cell.Coord.Column);
			for (int i = 0; i < 2; i++)
			{
				Assert.AreEqual(deps[i].Row, cell.DependentOn[i].Row);
				Assert.AreEqual(deps[i].Column, cell.DependentOn[i].Column);
			}
		}
		
		[TestMethod]
		public void GenerateNew_CorrectDivisionFormula()
		{
			FormulaCell cell = FormulaCell.GenerateNew("=AD14/B23", 16, 9);
			var deps = new[] {new Position(14, 30), new Position(23, 2)};
			
			Assert.IsNotNull(cell);
			Assert.AreEqual('/', cell.Operator);
			Assert.AreEqual(Status.CORRECT, cell.Status);
			Assert.AreEqual(16, cell.Coord.Row);
			Assert.AreEqual(9, cell.Coord.Column);
			for (int i = 0; i < 2; i++)
			{
				Assert.AreEqual(deps[i].Row, cell.DependentOn[i].Row);
				Assert.AreEqual(deps[i].Column, cell.DependentOn[i].Column);
			}
		}
		
		[TestMethod]
		public void GenerateNew_TwoCharacterOperands()
		{
			FormulaCell cell = FormulaCell.GenerateNew("=A4+B2", 16, 9);
			var deps = new[] {new Position(4, 1), new Position(2, 2)};
			
			Assert.IsNotNull(cell);
			Assert.AreEqual('+', cell.Operator);
			Assert.AreEqual(Status.CORRECT, cell.Status);
			Assert.AreEqual(16, cell.Coord.Row);
			Assert.AreEqual(9, cell.Coord.Column);
			for (int i = 0; i < 2; i++)
			{
				Assert.AreEqual(deps[i].Row, cell.DependentOn[i].Row);
				Assert.AreEqual(deps[i].Column, cell.DependentOn[i].Column);
			}
		}
	}

	[TestClass]
	public class SheetWriter_tests
	{
		class EmptyArgReader : IArgReader
		{
			public int Read()
			{
				return -1;
			}

			public string ReadArgument()
			{
				return null;
			}
		}
		
		class MockOutput : TextWriter
		{
			public override Encoding Encoding { get; }
			public string Result = "";
			
			public override void Write(string value)
			{
				Result = string.Concat(Result, value);
			}
		}

		[TestMethod]
		public void WriteSheet_EmptySheet()
		{
			Sheet sheet = Sheet.GenerateSheet(new EmptyArgReader());
			var output = new MockOutput();
			var writer = new SheetWriter(output);
			writer.WriteSheet(sheet);
			
			Assert.AreEqual("", output.Result);
		}
*/
	}
}