using System;
using System.Collections.Generic;
using System.IO;

namespace WordCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Argument Error");
                return;
            }
            
            try
            {
                WordProcessor words = new WordProcessor(args[0]);
                words.listWordCounts();
            }
            catch (Exception e)
            {
                Console.WriteLine("File Error");
            }
        }
    }

    class WordProcessor
    {
        private Reader reader;
        private SortedDictionary<string, int> wordCounts;

        public WordProcessor(string filePath)
        {
            reader = new Reader(filePath);
            wordCounts = new SortedDictionary<string, int>();
        }

        public int getWordCount()
        {
            int result = 0;
            while (reader.readWord() != "")
            {
                result++;
            }

            return result;
        }

        public void listWordCounts()
        {
            string word = reader.readWord();
            while (word != "")
            {
                updateValue(word);
                word = reader.readWord();
            }

            writeCounts();
        }

        void updateValue(string word)
        {
            bool exists = wordCounts.TryGetValue(word, out int occurences);
            wordCounts.Remove(word);
            wordCounts.Add(word, exists ? occurences+1 : 1);
        }

        void writeCounts()
        {
            foreach (var word in wordCounts)
            {
                Console.WriteLine(word.Key + ": " + word.Value);
            }
        }


    }


    class Reader
    {
        static readonly char[] Whitespaces = {' ', '\t', '\n'};
        private StreamReader streader;

        public Reader(string filePath)
        {
            streader = new StreamReader(filePath);
        }

        bool isWhitespace(int character)
        {
            foreach (char space in Whitespaces)
                if (space == character)
                    return true;
            return false;
        }

        public string readWord()
        {
            string nextWord = "";
            int c = streader.Read();
            while (isWhitespace(c))
                c = streader.Read();

            while (c != -1 && !isWhitespace(c))
            {
                nextWord = nextWord + (char) c;
                c = streader.Read();
            }

            return nextWord;
        }
    }
}
