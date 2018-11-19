using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime;
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
            /**/
            if (args.Length != 1)
            {
                ThrowArgumentError();
                return;
            }
            /**/
            
            FileStream inputStream = null;
            FileStream outputStream = null;
            try
            {
                inputStream = new FileStream(args[0], FileMode.Open);
                outputStream = new FileStream(args[0] + ".huff", FileMode.OpenOrCreate);
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
            
            var reader = new FileStreamInput(inputStream);
            var writer = new FileStreamOutput(outputStream);
            var analyzer = new FrequencyAnalyzer();
            var creator = new HuffmanTreeCreator();
            
            var algorithm = new HuffmanAlgorithm(analyzer, creator);
            algorithm.Encode(reader, writer);
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
    public class HuffmanAlgorithm
    {
        private ISymbolAnalyzer _analyzer;
        private ITreeCreator _creator;
        
        public HuffmanAlgorithm(ISymbolAnalyzer analyzer, ITreeCreator creator)
        {
            _analyzer = analyzer;
            _creator = creator;
        }

        public void Encode(IFileInput input, IFileOutput output)
        {
            int symbol;

            while ((symbol = input.ReadByte()) != -1)
            {
                _analyzer.ProcessSymbol(symbol);
            }

            long[] data = _analyzer.GetAnalysis();
            BinaryNode root = _creator.CreateTree(data);
            var encoder = new HuffmanEncoder(output, root);
            
            input.GoToStart();
            encoder.Encode(input);
            encoder.Finish();
        }
    }
    
    /// <summary>
    /// Counts the frequency of given symbols
    /// </summary>
    public class FrequencyAnalyzer : ISymbolAnalyzer
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

    public class HuffmanTreeCreator : ITreeCreator
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
   /* public class HuffmanTreeWriter : ITreeWriter
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
                WriteTree(root.Left);
                WriteTree(root.Right);
            }
        }
    }*/

    public class HuffmanEncoder
    {
        private IFileOutput _output;
        private BinaryNode _tree;
        private bool[][] _paths = new bool[256][];
        private Queue<bool> _buffer;
        
        public HuffmanEncoder(IFileOutput output, BinaryNode huffmanTree)
        {
            _output = output;
            _tree = huffmanTree;
        }

        public void Encode(IFileInput input)
        {
            _buffer = new Queue<bool>(520);
            WriteEncodingHeader();
            WriteTree(_tree);
            _output.Write(new byte[8], 0, 8);
            CreatePaths(_tree, new bool[0], 0);

            int c;
            while ((c = input.ReadByte()) != -1)
            {
                EncodeByte((byte) c);
            }
        }

        void WriteEncodingHeader()
        {
            byte[] header = new byte[]{0x7B, 0x68, 0x75, 0x7C, 0x6D, 0x7D, 0x66, 0x66};
            _output.Write(header, 0, header.Length);
        }

        internal void WriteTree(BinaryNode current)
        {
            if (current == null) return;
            
            ulong encoded, key;
            ulong weight = (((ulong) current.Weight) << 9) >> 8;
            
            if (current.isLeaf())
            {
                encoded = 1;
                key = ((ulong) current.Key) << 56;
            }
            else
            {
                encoded = 0;
                key = 0;
            }

            encoded = encoded | weight | key;
            byte[] output = BitConverter.GetBytes(encoded);
            _output.Write(output, 0, output.Length);
            
            WriteTree(current.Left);
            WriteTree(current.Right);
        }

        void CreatePaths(BinaryNode current, bool[] soFar, int depth)
        {
            if (current.isLeaf())
            {
                _paths[current.Key] = soFar;
                return;
            }
            
            bool[] child = new bool[depth+1];
            Array.Copy(soFar, child, soFar.Length);
            child[depth] = false;
            CreatePaths(current.Left, child, depth+1);


            child = new bool[depth + 1];
            Array.Copy(soFar, child, soFar.Length);
            child[depth] = true;
            CreatePaths(current.Right, child, depth+1);
        }

        void EncodeByte(byte value)
        {
            bool[] path = _paths[value];
            for (int i = 0; i < path.Length; i++)
            {
                _buffer.Enqueue(path[i]);
            }

            while (_buffer.Count >= 8)
            {
                WriteByteFromBuffer();
            }
        }

        void WriteByteFromBuffer()
        {
            uint value = 0;
            for (int i = 0; i < 8; i++)
            {
                if (_buffer.Count != 0 && _buffer.Dequeue())
                {
                    value = value | (uint) (1 << i);
                }
            }
            
            _output.WriteByte((byte) value);
        }

        public void Finish()
        {
            while (_buffer.Count > 0)
            {
                WriteByteFromBuffer();
            }
        }
    }
    
    /// <summary>
    /// Represents one node of a binary tree.
    /// </summary>
    public class BinaryNode
    {
        public int Key;
        public long Weight;
        public BinaryNode Left;
        public BinaryNode Right;

        public BinaryNode(int key, long weight, BinaryNode left, BinaryNode right)
        {
            Key = key;
            Weight = weight;
            Left = left;
            Right = right;
        }

        public BinaryNode(BinaryNode left, BinaryNode right)
        {
            Left = left;
            Right = right;
        }

        public bool isLeaf()
        {
            return Left == null;
        }
    }
    
    /// <summary>
    /// A priority queue for Huffman tree nodes. Prioritizes by weight of the node.
    /// </summary>
    public class HuffmanPriorityQueue : IPriorityQueue<BinaryNode>
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

    public class FileStreamInput : IFileInput
    {
        private FileStream _stream;
        
        public FileStreamInput(FileStream stream)
        {
            _stream = stream;
        }
        
        public int ReadByte()
        {
            return _stream.ReadByte();
        }

        public void GoToStart()
        {
            _stream.Position = 0;
        }
    }

    public class FileStreamOutput : IFileOutput
    {
        private FileStream _stream;

        public FileStreamOutput(FileStream stream)
        {
            _stream = stream;
        }

        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        public void Write(byte[] array, int offset, int count)
        {
            _stream.Write(array, offset, count);
        }
    }
    
    public interface IFileInput
    {
        int ReadByte();
        void GoToStart();
    }

    public interface IFileOutput
    {
        void WriteByte(byte output);
        void Write(byte[] array, int offset, int count);
    }
    
    public interface IPriorityQueue<T>
    {
        void Add(T value);
        T ExtractMin();
    }

    public interface ISymbolAnalyzer
    {
        void ProcessSymbol(int symbol);
        long[] GetAnalysis();
    }

    public interface ITreeCreator
    {
        BinaryNode CreateTree(long[] data);
    }

    public interface ITreeWriter
    {
        void WriteTree(BinaryNode root);
    }
    
    
}