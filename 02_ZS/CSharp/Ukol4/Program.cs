using System;
using System.Collections.Generic;
using System.IO;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Ukol4_tests")]
namespace Ukol4
{
    class Program
    {
        const string option = "--highlight-spaces";
        static void Main(string[] args)
        {
            #region Argument checks
            if (args.Length < 3 || (args[0] == option && args.Length < 4))
            {
                ThrowErrorMessage(0);
                return;
            }

            bool goodLength = int.TryParse(args[args.Length-1], out int length);
            if (!goodLength || length <= 0)
            {
                ThrowErrorMessage(0);
                return;
            }
            #endregion

            int inputStart = 0;                     // where input file arguments start
            LineReader reader = new LineReader();
            
            // Setting up output writer and word processor
            StreamWriter output = GetOutputSafely(args[args.Length-2]);
            if (output == null)
            {
                ThrowErrorMessage(1);
                return;
            }
            BlockAligner processor = new BlockAligner(length, output);
            if (args[0] == "--highlight-spaces")
            {
                processor.HighlightSpaces = true;
                inputStart++;
            }

            // Running the algorithm on all files
            var algorithm = new WordProcessingAlgorithm(reader, processor);
            StreamReader input = null;
            for (int i = inputStart; i < args.Length - 2; i++)
            {
                input = GetInputSafely(args[i]);
                if (input == null) continue;
                reader.SetInput(input);
                algorithm.RunWithoutFinish();
                input.Close();
            }
            algorithm.Finish();
            output.Close();
            
        }

        /// <summary>
        /// Writes an appropriate error message to the console. 
        /// </summary>
        /// <param name="errorCode"> 
        /// Code of the error to be thrown. 
        /// 0 for argument error, 1 for file error. </param>
        static void ThrowErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case 0:
                    Console.WriteLine("Argument Error");
                    break;
                case 1:
                    Console.WriteLine("File Error");
                    break;
            }
        }

        /// <summary>
        /// Tries to create a StreamReader with specified input path. Returns null if creation fails.
        /// </summary>
        /// <param name="path">path of the file to be opened by reader</param>
        /// <returns>StreamReader reading from specified path, or null if impossible</returns>
        static StreamReader GetInputSafely(string path)
        {
            StreamReader ret = null;
            try
            {
                ret = new StreamReader(path);
            }
            catch (ArgumentException){ }
            catch (FileNotFoundException){ }
            catch (DirectoryNotFoundException){ }
            catch (IOException){ }

            return ret;
        }

        /// <summary>
        /// Tries to create a StreamWriter with specified output path. Returns null if creation fails.
        /// </summary>
        /// <param name="path">path of the file to be written to by writer</param>
        /// <returns>StreamWriter writing to file specified by path, or null if impossible</returns>
        static StreamWriter GetOutputSafely(string path)
        {
            StreamWriter ret = null;
            try
            {
                ret = new StreamWriter(path);
            }
            catch (UnauthorizedAccessException){ }
            catch (ArgumentException){ }
            catch (DirectoryNotFoundException){ }
            catch (PathTooLongException){ }
            catch (IOException){ }
            catch (System.Security.SecurityException){ }

            return ret;
        }
    }

    class WordProcessingAlgorithm
    {
        IWordProcessor Processor;   // Word Processor to be used in the algorithm
        IReader Reader;             // Reader to provide input words

        public WordProcessingAlgorithm(IReader reader, IWordProcessor processor)
        {
            Processor = processor;
            Reader = reader;
        }
        

        public void Run()
        {
            RunWithoutFinish();
            Processor.Finish();
        }

        public void RunWithoutFinish()
        {
            Processor.Start();
            string word;

            while ((word = Reader.ReadWord()) != null) 
            {
                Processor.ProcessWord(word);
            }
        }

        public void Finish()
        {
            Processor.Finish();
        }
    }

    /// <summary>
    /// Reads words from input. Stops on every newline.
    /// </summary>
    class LineReader : IReader
    {
        public static readonly string EOFString = null;
        static char[] whitespaces = new[] {'\t', ' ', '\n'};
        TextReader Input;
        bool foundNewline;      // True if '\n' was the last character read on last read;
        
        #region Constructors
        public LineReader(TextReader input)
        {
            Input = input ?? Console.In;
            foundNewline = false;
        }

        public LineReader()
        {
            Input = Console.In;
            foundNewline = false;
        }
        #endregion
        // Detects if a character belongs to the whitespaces array
        bool isWhitespace(int c)
        {
            foreach (char whitespace in whitespaces)
                if (c == whitespace)
                    return true;

            return false;
        }

        /// <summary>
        /// Reads the next word from input, skipping whitespaces. Stops on newline.
        /// </summary>
        /// <returns>
        /// "\n" string if newline was encountered, otherwise a string containing the next word.
        /// </returns>
        public string ReadWord()
        {
            if (foundNewline)
            {
                foundNewline = false;
                return "\n";
            }

            string word = "";
            int charRead = Input.Read();

            while (isWhitespace(charRead))
            {
                if ((char) charRead == '\n')
                    return "\n";
                charRead = Input.Read();
            }

            while (charRead != -1 && !isWhitespace(charRead))
            {
                word = word + (char) charRead;
                charRead = Input.Read();
            }

            if ((char) charRead == '\n')
            {
                foundNewline = true;
            }

            return word == "" ? null : word;
        }

        public void SetInput(TextReader input)
        {
            Input = input;
        }
    }

    // Aligns words into specified blocks. 
    class BlockAligner : IWordProcessor
    {
        int BlockSize;                      // Total width of the desired block
        int totalWordLength;                // Total length of all words in queue
        int newLinesRead;                   // Newlines read in sequence without any word
        public bool HighlightSpaces;        // Determines if spaces and newlines should be highlighted
        TextWriter Output;                  
        Queue<string> line;                 // Queue of the words to be put on next line

        #region Constructors
        public BlockAligner(int blockSize)
        {
            BlockSize = blockSize;
            line = new Queue<string>();
        }

        public BlockAligner(int blockSize, TextWriter output)
        {
            BlockSize = blockSize;
            line = new Queue<string>();
            SetOutput(output);
        }

        public BlockAligner(int blockSize, TextWriter output, bool highlight)
        {
            BlockSize = blockSize;
            line = new Queue<string>();
            SetOutput(output);
            HighlightSpaces = highlight;
        }
        #endregion

        public void SetOutput(TextWriter output)
        {
            Output = output ?? Console.Out;
        }

        public void Start(){}

        public void ProcessWord(string word)
        {
            if (word == "\n")               // newline encountered
            {
                newLinesRead++;
                return;
            }

            if (newLinesRead > 1)           // word after multiple newlines -> new paragraph
            {
                WriteEndOfParagraph();
                MyWriteLine();         // next paragraph is following -> newline needed
            }

            newLinesRead = 0;
            if (word.Length + totalWordLength + line.Count > BlockSize)     // next word doesn't fit in line
            {
                WriteNextLine();            
            }

            AddWord(word);
        }

        public void Finish()
        {
            WriteEndOfParagraph();   
        }

        // Writes the last, left-aligned line of a paragraph.
        void WriteEndOfParagraph()
        {
            if (line.Count == 0) return;    // If file ended after the end of a paragraph, line would be empty

            while (line.Count > 1)
            {
                Output.Write(line.Dequeue());
                FillSpaces(1);
            }
            MyWriteLine(line.Dequeue());
                        
            totalWordLength = 0;
        }

        // Writes the next line in specified block-alignment
        void WriteNextLine()
        {
            if (line.Count == 0) return;
            int spaceCount = line.Count - 1;                // number of whitespace sequences between words
            int totalSpaces = 0, normalSpace = 0, extraSpaces = 0;

            if (spaceCount > 0)                             // single word doesn't get aligned
            {
                totalSpaces = BlockSize - totalWordLength;  // number of space characters to be written
                normalSpace = totalSpaces / (spaceCount);   // number of spaces in each whitespace sequence
                extraSpaces = totalSpaces - normalSpace * (spaceCount);  // number of sequences getting an extra space (if not uniform)
            }

            for (int i = 0; i < spaceCount; i++)
            {
                Output.Write(line.Dequeue());
                FillSpaces(i < extraSpaces ? normalSpace + 1 : normalSpace); // first "extraSpaces" sequences get an extra space
            }
            MyWriteLine(line.Dequeue());

            totalWordLength = 0;
        }

        // Adds a word to queue and updates totalWordLength
        void AddWord(string word)
        {
            line.Enqueue(word);
            totalWordLength += word.Length;
        }

        // Writes the desired number of spaces to Output
        void FillSpaces(int spaceCount)
        {
            for (int i = 0; i < spaceCount; i++)
            {
                Output.Write(HighlightSpaces ? "." : " ");
            }
        }

        /// <summary>
        /// Works as WriteLine, but adds EOL highlight characters if desired
        /// </summary>
        /// <param name="value">string to be written to Output</param>
        void MyWriteLine(string value)
        {
            Output.Write(value);
            MyWriteLine();
        }
        void MyWriteLine()
        {
            Output.WriteLine(HighlightSpaces ? "<-" : "");
        }

        

    }


    interface IWordProcessor
    {
        void Start();

        void SetOutput(TextWriter output);

        void ProcessWord(string word);

        void Finish();
    }

    interface IReader
    {
        string ReadWord();

        void SetInput(TextReader input);
    }
}
