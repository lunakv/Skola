using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;
using System.IO;

using LibraryModel;

namespace MergeSortQuery {
	class MergeSortQuery
	{
		public Library Library { get; set; }
		public int ThreadCount { get; set; }

		public List<Copy> ExecuteQuery()
		{
			if (ThreadCount == 0)
				throw new InvalidOperationException("Threads property not set and default value 0 is not valid.");

			bool Filter(Copy copy)
			{
				var shelf = copy.Book.Shelf;
				return copy.State == CopyState.OnLoan && shelf[shelf.Length - 1] >= 'A' && shelf[shelf.Length - 1] <= 'Q';
			}
			
			var filteredCopies = FilterCopies(Library.Copies, Filter);
			var sorter = new ParallelSorter(0, filteredCopies.Count){Copies = filteredCopies, ThreadCount = ThreadCount};
			sorter.Sort();
			return sorter.Result;
		}

		private List<Copy> FilterCopies(List<Copy> copies, Predicate<Copy> filter)
		{
			var ret = new List<Copy>();
			foreach (var copy in copies)
			{
				if (filter(copy))
				{
					ret.Add(copy);
				}
			}

			return ret;
		}
	}

	class ParallelSorter
	{
		public int ThreadCount { get; set; }
		public List<Copy> Copies { get; set; }
		public List<Copy> Result { get; private set; }
		
		private int _startI, _count;
		private static Comparer<Copy> _comparer = new LoanedBooksComparer();
		
		public ParallelSorter(int start, int count)
		{
			_startI = start;
			_count = count;
		}

		public void Sort()
		{
			if (ThreadCount == 0)
			{
				throw new InvalidOperationException("Threads property not set and default value 0 is not valid.");
			}
			
			if (ThreadCount == 1)
			{
				Result = Copies.GetRange(_startI, _count);
				Result.Sort(_comparer);
			}
			else
			{
				var leftSon = new ParallelSorter(_startI, _count/2);
				leftSon.ThreadCount = ThreadCount / 2;
				leftSon.Copies = Copies;
				var leftThread = new Thread(leftSon.Sort);
				var rightSon = new ParallelSorter(_startI + _count / 2, _count - _count / 2);
				rightSon.ThreadCount = (ThreadCount + 1) / 2;
				rightSon.Copies = Copies;
				var rightThread = new Thread(rightSon.Sort);

				leftThread.Start();
				rightThread.Start();

				leftThread.Join();
				rightThread.Join();
				Result = new List<Copy>();
				Merge(leftSon.Result, rightSon.Result);
			}
		}

		public void Merge(List<Copy> a, List<Copy> b)
		{
			int iA = 0, iB = 0;
			bool done = false;
			
			while (!done)
			{
				if (iA == a.Count && iB == b.Count)
				{
					done = true;
				}
				else if (iA == a.Count)
				{
					Result.Add(b[iB++]);
				}
				else if (iB == b.Count)
				{
					Result.Add(a[iA++]);
				}
				else if (_comparer.Compare(a[iA], b[iB]) <= 0)
				{
					Result.Add(a[iA++]);
				}
				else
				{
					Result.Add(b[iB++]);
				}
			}
		}

		private class LoanedBooksComparer : Comparer<Copy>
		{
			public override int Compare(Copy x, Copy y)
			{
				if (x == null && y == null) return 0;
				if (x == null) return -1;
				if (y == null) return 1;
				int comp;
				if ((comp = x.OnLoan.DueDate.CompareTo(y.OnLoan.DueDate)) != 0) return comp;
				if ((comp = string.Compare(x.OnLoan.Client.LastName, y.OnLoan.Client.LastName, StringComparison.CurrentCulture)) != 0) return comp;
				if ((comp = string.Compare(x.OnLoan.Client.FirstName, y.OnLoan.Client.FirstName, StringComparison.CurrentCulture)) != 0) return comp;
				if ((comp = string.Compare(x.Book.Shelf, y.Book.Shelf, StringComparison.Ordinal)) != 0) return comp;
				if ((comp = string.Compare(x.Id, y.Id, StringComparison.Ordinal)) != 0) return comp;
				return 0;
			}
		}
	}
	
	
	
	

	class ResultVisualizer {
		public static void PrintCopy(StreamWriter writer, Copy c) {
			writer.WriteLine("{0} {1}: {2} loaned to {3}, {4}.", c.OnLoan.DueDate.ToShortDateString(), c.Book.Shelf, c.Id, c.OnLoan.Client.LastName, System.Globalization.StringInfo.GetNextTextElement(c.OnLoan.Client.FirstName));
		}
	}
}
