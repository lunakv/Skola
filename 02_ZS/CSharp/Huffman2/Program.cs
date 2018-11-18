using System;
using System.Collections.Generic;
using System.IO;
using System.Security;

namespace Huffman2
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                ThrowArgumentError();
                return;
            }

            FileStream input = null, output = null;

            try
            {
                input = new FileStream(args[0], FileMode.Open);
                output = new FileStream(args[0] + ".huff", FileMode.Create);
            }
            catch (Exception e) when (e is ArgumentException || e is NotSupportedException || e is SecurityException ||
                                      e is FileNotFoundException || e is IOException ||
                                      e is DirectoryNotFoundException ||
                                      e is PathTooLongException || e is ArgumentOutOfRangeException)
            {
                ThrowFileError();
                return;
            }
            
            var encoder = new Huffman(input, output);
            encoder.Encode();
            
            input.Dispose();
            output.Dispose();
        }

        private static void ThrowFileError()
        {
            Console.Write("File Error.");
        }

        private static void ThrowArgumentError()
        {
            Console.Write("Argument Error.");
        }
    }

    /// <summary>
    /// Class used for Huffman encoding. Converts a byte stream to an encoded byte stream. 
    /// </summary>
    public class Huffman
    {
        private Stream _input, _output;
        private FileStreamReader _reader;
        
        public Huffman(Stream input, Stream output)
        {
            _input = input;
            _output = output;
            _reader = new FileStreamReader(_input);
        }

        public void Encode()
        {
            var counter = new FrequencyCounter(_reader);
            long[] frequencies = counter.GetByteFrequencies();
            
            var treeMaker = new HuffmanTreeMaker();
            Node tree = treeMaker.CreateHuffmanTree(frequencies);

            _reader.GoToStart();
            var writer = new HuffmanEncoder(_reader, _output);
            writer.CompressStream(tree);
        }
    }

    /// <summary>
    /// Given a Huffman tree and an input stream reader, produces an encoded stream to a provided output stream;
    /// </summary>
    public class HuffmanEncoder
    {
        private Stream _outStream;
        private FileStreamReader _reader;
        private bool[][] _bytePaths = new bool[256][]; // Bool arrays specifying paths to each index through the tree
                                                       //     left edge = false, right edge = true
        private Queue<bool> _buffer;                   // Buffer of bits to be written to output
        
        public HuffmanEncoder(FileStreamReader reader, Stream output)
        {
            _outStream = output;
            _reader = reader;
        }

        /// <summary>
        /// Compresses the incoming file stream according to a constructed Huffman tree.
        /// </summary>
        /// <param name="huffmanTree">
        /// Huffman tree with calculated frequencies to be used for encoding.
        /// </param>
        public void CompressStream(Node huffmanTree)
        {
            _buffer = new Queue<bool>();
            
            // Writes header and encoded tree to output.
            CreateBytePaths(huffmanTree, new bool[0], 0);
            WriteEncodingHeader();
            WriteEncodedTree(huffmanTree);
            _outStream.Write(new byte[8], 0, 8); 

            // Main encoding loop
            int nextByte;
            while ((nextByte = _reader.Read()) != -1)
            {
                AddToBuffer(nextByte);
                WriteBuffer(false);
            }
            
            WriteBuffer(true);
        }

        private void AddToBuffer(int nextByte)
        {
            for (int i = 0; i < _bytePaths[nextByte].Length; i++)
            {
                _buffer.Enqueue(_bytePaths[nextByte][i]);
            }
        }

        private void WriteBuffer(bool flushBuffer)
        {
            while (_buffer.Count >= 8)
            {
                int output = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (_buffer.Dequeue())
                    {
                        output |= (1 << i);
                    }
                }
                _outStream.WriteByte((byte) output);
            }
            
            if (flushBuffer) WriteLastByte();
        }

        private void WriteLastByte()
        {
            int count = _buffer.Count;
            int output = 0;
            for (int i = 0; i < count; i++)
            {
                if (_buffer.Dequeue())
                    output |= (1 << i);
            }
            
            _outStream.WriteByte((byte) output);
        }
        
        /// <summary>
        /// Creates all arrays specifying byte paths from root to each leaf. 
        /// </summary>
        /// <param name="current"> Root of the tree to be searched </param>
        /// <param name="code"> Intermediate path value. Use <code>new bool[0]</code> for root. </param>
        /// <param name="depth"> Depth of the the <paramref name="current"/> node in the tree. 0 for root.</param>
        private void CreateBytePaths(Node current, bool[] code, int depth)
        {
            if (current.isLeaf())
            {
                _bytePaths[current.Key] = code;
                return;
            }
            
            // Extends array by left edge and calls itself on left son
            bool[] leftCode = new bool[depth + 1];
            Array.Copy(code, leftCode, depth);
            leftCode[depth] = false;
            CreateBytePaths(current.Left, leftCode, depth + 1);

            // Extends array by right edge and calls itself on right son
            bool[] rightCode = (bool[]) leftCode.Clone();
            rightCode[depth] = true;
            CreateBytePaths(current.Right, rightCode, depth + 1);
        }

        private void WriteEncodingHeader()
        {
            byte[] header = new byte[] {0x7B, 0x68, 0x75, 0x7C, 0x6D, 0x7D, 0x66, 0x66 };
            _outStream.Write(header, 0, header.Length);
        }
        
        /// <summary>
        /// Writes to output an encoded Huffman tree in prefix notation.
        /// </summary>
        /// <param name="node">
        /// Root of the tree to be written.
        /// </param>
        private void WriteEncodedTree(Node node)
        {
            if (node == null) return;
            
            ulong treeCode = node.isLeaf() ? (ulong) 1 : 0; // bit 0: 1 for leaves only
            byte key = node.Key;                            // bits 56 - 63: symbol value for leaves, 0 otherwise
            ulong value = (((ulong) node.Value) << 9) >> 9; // bits 1 - 55: first 55 bits of node value

            treeCode = treeCode | (value << 1) | ((ulong) key << 56);
            byte[] output = BitConverter.GetBytes(treeCode);
            _outStream.Write(output, 0, output.Length);
            
            WriteEncodedTree(node.Left);
            WriteEncodedTree(node.Right);
        }
        
    }
    
    /// <summary>
    /// Counts frequencies of each byte in an input stream.
    /// </summary>
    public class FrequencyCounter
    {
        private long[] _frequencies;
        private FileStreamReader _reader;

        public FrequencyCounter(FileStreamReader input)
        {
            _reader = input;
        }
        
        /// <summary>
        /// Reads file from <code>input</code> stream and counts the occurrences of each byte.
        /// </summary>
        /// <returns>
        /// An array of longs indexed by byte codes with the number of occurrences for each.
        /// </returns>
        public long[] GetByteFrequencies()
        {
            _frequencies = new long[256];
            int nextByte;
            
            while ((nextByte = _reader.Read()) != -1)
            {
                _frequencies[nextByte]++;
            }

            return _frequencies;
        }
    }
    
    /// <summary>
    /// Reads bytes from an input stream.
    /// </summary>
    public class FileStreamReader : IDisposable
    {
        private byte[] _buffer = new byte[4096];
        private int _index = 0, _bytesRead = 0;
        private Stream _input;
        
        public FileStreamReader(Stream input)
        {
            _input = input;
        }

        /// <summary>
        /// Sets the current position to the start of the stream.
        /// </summary>
        public void GoToStart()
        {
            _input.Position = 0;
        }
        
        /// <summary>
        /// Reads the next byte from input.
        /// </summary>
        /// <returns>
        /// The next character from the input stream, or -1 if the end was reached. 
        /// </returns>
        public int Read()
        {
            if (_index == _bytesRead)
            {
                GetNewBuffer();
                if (_bytesRead == 0) return -1;
            }

            return _buffer[_index++];
        }

        private void GetNewBuffer()
        {
            _bytesRead = _input.Read(_buffer, 0, _buffer.Length);
            _index = 0;
        }

        public void Dispose()
        {
            _input.Dispose();
        }
    }
   
    
    public class HuffmanTreeMaker
    {
        private Queue<Node> _leafQueue = new Queue<Node>();
        private Queue<Node> _internalQueue = new Queue<Node>();

        /// <summary>
        /// Creates a Huffman tree given a table of frequencies.
        /// </summary>
        /// <param name="frequencies"> Table of frequencies of each byte. </param>
        /// <returns> A root node of the created Huffman tree. </returns>
        public Node CreateHuffmanTree(long[] frequencies)
        {
            CreateLeafQueue(frequencies);

            while (_leafQueue.Count > 0 || _internalQueue.Count > 1)
            {
                Node first = GetMinimumNode();
                Node second = GetMinimumNode();
                
                var intern = new Node(0, first.Value + second.Value, first, second);
                _internalQueue.Enqueue(intern);
            }

            return _internalQueue.Dequeue();
        }
        
        private Node GetMinimumNode()
        {
            if (_leafQueue.Count == 0) return _internalQueue.Dequeue();
            if (_internalQueue.Count == 0) return _leafQueue.Dequeue();

            Node leaf = _leafQueue.Peek();
            Node intern = _internalQueue.Peek();

            return leaf < intern ? _leafQueue.Dequeue() : _internalQueue.Dequeue();
        }

        /// <summary>
        /// Creates a sorted queue of leaf nodes for the tree building algorithm.
        /// </summary>
        /// <param name="frequencies">
        /// Table of frequencies for each byte.
        /// </param>
        private void CreateLeafQueue(long[] frequencies)
        {
            var leavesAndNulls = new Node[256];
            int j = 0;
            
            for (int i = 0; i < frequencies.Length; i++)
            {
                if (frequencies[i] == 0) continue;
                leavesAndNulls[j++] = new Node((byte) i, frequencies[i], null, null);;
            }

            // Trimming nulls from the initial array to enable sorting.
            var justLeaves = new Node[j];
            Array.Copy(leavesAndNulls, justLeaves, j);
            
            Array.Sort(justLeaves);
            for (byte i = 0; i < justLeaves.Length; i++)
            {
                _leafQueue.Enqueue(justLeaves[i]);
            }
        }
    }
    
    /// <summary>
    /// One node of a Huffman tree.
    /// </summary>
    public class Node : IComparable
    {
        public byte Key;
        public long Value;
        public Node Left, Right;

        public Node(byte key, long value, Node left, Node right)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
        }

        public bool isLeaf()
        {
            return Left == null;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Node)) throw new NotImplementedException("Comparing Node to not Node is not supported.");
            var other = (Node) obj;
     
            // Node value is the primary criterion for comparison
            if (this.Value < other.Value) return -1;
            if (this.Value > other.Value) return +1;
            
            // With equal value, leaves take precedent over inner nodes
            if (this.isLeaf() && !other.isLeaf()) return -1;
            if (!this.isLeaf() && other.isLeaf()) return +1;
            
            // If both are leaves, compare by Key
            if (this.isLeaf() /*other.isLeaf() implied*/ && this.Key < other.Key) return -1;
            if (this.isLeaf() /*other.isLeaf() implied*/ && this.Key > other.Key) return +1;
            
            // If both are inner, compare by creation order - implicitly by Queue
            throw new NotImplementedException("Comparison on two inner nodes shouldn't ever happen.");
        }
        
        #region Operators
        public static bool operator <(Node a, Node b)
        {
            return a.CompareTo(b) < 0;
        } 
        
        public static bool operator >(Node a, Node b)
        {
            return a.CompareTo(b) > 0;
        }
        #endregion
    }
}