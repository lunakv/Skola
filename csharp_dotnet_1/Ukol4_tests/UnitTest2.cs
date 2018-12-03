using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ukol4;

namespace Ukol4_tests
{

    public class MockInput : TextReader
    {
        string inputline;
        int i = 0;

        public MockInput(string input)
        {
            inputline = input;
        }

        public override int Read()
        {
            return i < inputline.Length ? inputline[i++] : -1;
        }
    }

    [TestClass]
    public class Reader_Tests
    {
        [TestMethod]
        public void ReadWord_OnlyWhitespace()
        {
            var inp = new MockInput("   \t  \t    \t\t\t ");
            var r = new LineReader(inp);

            List<string> words = new List<string>();
            for (int i = 0; i < 2; i++)
            {
                words.Add(r.ReadWord());
            }

            CollectionAssert.AreEqual(new string[]{null, null}, words);
        }

        [TestMethod]
        public void ReadWord_OnlyWhitespaceFollowedByNewline()
        {
            var inp = new MockInput("   \t  \t    \t\t\t  \n");
            var r = new LineReader(inp);

            List<string> words = new List<string>();
            for (int i = 0; i < 2; i++)
            {
                words.Add(r.ReadWord());
            }

            CollectionAssert.AreEqual(new []{"\n", null}, words);
        }

        [TestMethod]
        public void ReadWord_OnlyNonWhitespace()
        {
            string line = "ThisisoneLonWordThatTheReaderShouldReadAsAWord";
            var inp = new MockInput(line);
            var r = new LineReader(inp);

            List<string> words = new List<string>();
            for (int i = 0; i < 2; i++)
            {
                words.Add(r.ReadWord());
            }

            CollectionAssert.AreEqual(new []{line, null}, words);
        }

        [TestMethod]
        public void ReadWord_OnlyNonWhitespaceAndNewLine()
        {
            string word = "ThisisoneLonWordThatTheReaderShouldReadAsAWord";
            string line = word + "\n";
            var inp = new MockInput(line);
            var r = new LineReader(inp);

            List<string> words = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                words.Add(r.ReadWord());
            }

            CollectionAssert.AreEqual(new []{word, "\n", null}, words);
        }

        [TestMethod]
        public void ReadWord_WhiteSpaceFollowedByWord()
        {
            string word = "SomeWordToRead";
            string line = "   \t\t   \t " + word;
            var inp = new MockInput(line);
            var r = new LineReader(inp);

            List<string> words = new List<string>();
            for (int i = 0; i < 2; i++)
            {
                words.Add(r.ReadWord());
            }

            CollectionAssert.AreEqual(new []{word, null}, words);
        }

        [TestMethod]
        public void ReadWord_WordNewLineWord()
        {
            string word = "SomeWordToRead";
            string word2 = "SomeOtherWord";
            string line = word + "\n" + word2;
            var inp = new MockInput(line);
            var r = new LineReader(inp);

            List<string> words = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                words.Add(r.ReadWord());
            }

            CollectionAssert.AreEqual(new []{word, "\n", word2, null}, words);
        }

        [TestMethod]
        public void ReadWord_WordWhiteSpaceWord()
        {
            string word = "SomeWordToRead";
            string word2 = "SomeOtherWord";
            string line = word + "\t  \t\t   " + word2;
            var inp = new MockInput(line);
            var r = new LineReader(inp);

            List<string> words = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                words.Add(r.ReadWord());
            }

            CollectionAssert.AreEqual(new []{word, word2, null}, words);
        }

        [TestMethod]
        public void ReadWord_WordWhiteSpaceNewlineWord()
        {
            string word = "SomeWordToRead";
            string word2 = "SomeOtherWord";
            string line = word + "\t  \t\t   \n" + word2;
            var inp = new MockInput(line);
            var r = new LineReader(inp);

            List<string> words = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                words.Add(r.ReadWord());
            }

            CollectionAssert.AreEqual(new []{word, "\n", word2, null}, words);
        }

        [TestMethod]
        public void ReadWord_WordWhitespaceNewlineWhitespaceWord()
        {
            string word = "SomeWordToRead";
            string word2 = "SomeOtherWord";
            string line = word + "\t  \t\t\n \t  " + word2;
            var inp = new MockInput(line);
            var r = new LineReader(inp);

            List<string> words = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                words.Add(r.ReadWord());
            }

            CollectionAssert.AreEqual(new []{word, "\n", word2, null}, words);
        }
    }
}
