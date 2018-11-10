using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace HuffmanTrees
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Run(args);
        }

        static void Run(string[] args)
        {
            
            if (args.Length != 1)
            {
                ThrowArgumentError();
                return;
            }

            FileStream reader = null;
            
            try
            {
                reader = new FileStream(args[0], FileMode.Open);
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is FileNotFoundException || e is DirectoryNotFoundException ||
                    e is IOException || e is NotSupportedException || e is SecurityException)
                {
                    ThrowFileError();
                    return;
                }
                
                throw;
            }

            var analyzer = new FrequencyAnalyzer();
            var creator = new HuffmanTreeCreator();
            var writer = new HuffmanTreeWriter(Console.Out);
            
            var algorithm = new HuffmanAlgorithm(reader, analyzer, creator, writer);
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
    /// Creates and immediately runs an instance of an algorithm to create and write a Huffman tree
    /// </summary>
    class HuffmanAlgorithm
    {
        public HuffmanAlgorithm(Stream reader, ISymbolAnalyzer analyzer, ITreeCreator creator, ITreeWriter writer)
        {
            int symbol;

            while ((symbol = reader.ReadByte()) != -1)
            {
                analyzer.ProcessSymbol(symbol);
            }

            long[] data = analyzer.GetAnalysis();
            BinaryNode root = creator.CreateTree(data);
            writer.WriteTree(root);
        }
    }
    
    /// <summary>
    /// Counts the frequency of given symbols
    /// </summary>
    class FrequencyAnalyzer : ISymbolAnalyzer
    {
        #region properties
        
        public const int MAX_SYMBOLS = 256;
        private long[] _frequencies = new long[MAX_SYMBOLS];

        #endregion
        #region methods
        
        /// <summary>
        /// Increases the counted frequency of a symbol by one.
        /// </summary>
        /// <param name="symbol">The symbol to be counted</param>
        public void ProcessSymbol(int symbol)
        {
            if (symbol >= 0 && symbol < MAX_SYMBOLS)
                _frequencies[symbol]++;
        }
        
        /// <summary>
        /// Returns an array of frequencies for each symbol
        /// </summary>
        /// <returns> an array indexed by symbols where each element contains the frequency of the symbol.</returns>
        public long[] GetAnalysis()
        {
            return _frequencies;
        }
        
        #endregion
    }

    class HuffmanTreeCreator : ITreeCreator
    {
        HuffmanPriorityQueue _processingNodes = new HuffmanPriorityQueue();
        
        /// <summary>
        /// Creates a Huffman tree from a frequency array.
        /// </summary>
        /// <param name="data">An array containing frequencies of each symbol</param>
        /// <returns>a BinaryNode object representing the root of the tree</returns>
        public BinaryNode CreateTree(long[] data)
        {
            BinaryNode root = null;
            
            // Adds the leafs into the queue
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0)
                {
                    _processingNodes.Add(new BinaryNode(i, data[i], null, null));
                }
            }
            
            // Processes the remaining nodes until only root remains
            while (_processingNodes.Count > 1)
            {
                BinaryNode left = _processingNodes.ExtractMin();
                BinaryNode right = _processingNodes.ExtractMin();
                _processingNodes.Add(new BinaryNode(-1, left.Weight + right.Weight, left, right));
            }

            root = _processingNodes.ExtractMin();
            return root;
        }
    }
    
    /// <summary>
    /// Writes a representation of a Huffman tree in prefix notation
    /// </summary>
    class HuffmanTreeWriter : ITreeWriter
    {
        private TextWriter _output;
        private bool _firstWrite;
        
        public HuffmanTreeWriter(TextWriter output)
        {
            _output = output;
            _firstWrite = true;
        }
        
        /// <summary>
        ///  Writes down, in prefix notation, all descendants of the specified node 
        /// </summary>
        /// <param name="root">The root of the tree to be written</param>
        public void WriteTree(BinaryNode root)
        {
            if (root == null) return;

            if (!_firstWrite)
                _output.Write(" ");
            else
                _firstWrite = false;                
            
            if (root.Key != -1)
            {
                _output.Write("*" + root.Key + ":" + root.Weight);
            }
            else
            {
                _output.Write(root.Weight);
                WriteTree(root.LeftSon);
                WriteTree(root.RightSon);
            }
        }
    }
    
    /// <summary>
    /// Represents one node of a binary tree.
    /// </summary>
    class BinaryNode
    {
        public int Key;
        public long Weight;
        public BinaryNode LeftSon;
        public BinaryNode RightSon;

        public BinaryNode(int key, long weight, BinaryNode leftSon, BinaryNode rightSon)
        {
            Key = key;
            Weight = weight;
            LeftSon = leftSon;
            RightSon = rightSon;
        }

        public BinaryNode(BinaryNode leftSon, BinaryNode rightSon)
        {
            LeftSon = leftSon;
            RightSon = rightSon;
        }
    }
    
    /// <summary>
    /// A priority queue for Huffman tree nodes. Prioritizes by weight of the node.
    /// </summary>
    class HuffmanPriorityQueue : IPriorityQueue<BinaryNode>
    {
        private LinkedList<BinaryNode> NodeQueue = new LinkedList<BinaryNode>();
        public int Count => NodeQueue.Count;

        /// <summary>
        /// Adds an item to the appropriate place in the queue. 
        /// </summary>
        /// <param name="value">The node to be added.</param>
        public void Add(BinaryNode value)
        {
            var currentNode = NodeQueue.Last;
            while (currentNode != null && currentNode.Value.Weight > value.Weight)
                currentNode = currentNode.Previous;

            if (currentNode == null)
            {
                NodeQueue.AddFirst(value);
            }
            else
            {
                NodeQueue.AddAfter(currentNode, value);
            }
        }

        /// <summary>
        /// Returns the item with the highest priority from the queue and removes it.
        /// </summary>
        /// <returns>The item with the highest priority in the queue, or null, if the queue is empty.</returns>
        public BinaryNode ExtractMin()
        {
            BinaryNode result = NodeQueue.First?.Value;
            
            NodeQueue.RemoveFirst();
            return result;
        }
    }
    
    interface IPriorityQueue<T>
    {
        void Add(T value);
        T ExtractMin();
    }

    interface ISymbolAnalyzer
    {
        void ProcessSymbol(int symbol);
        long[] GetAnalysis();
    }

    interface ITreeCreator
    {
        BinaryNode CreateTree(long[] data);
    }

    interface ITreeWriter
    {
        void WriteTree(BinaryNode root);
    }
    
    
}