using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ukol4;

namespace Ukol4_tests
{
    class MockReader : IReader
    {
        string[] words;
        int i = 0;

        public MockReader(string[] input)
        {
            words = input;
        }

        public string ReadWord()
        {
            return i < words.Length ? words[i++] : null;
        }

        public void SetInput(TextReader input)
        {
            throw new NotImplementedException();
        }
    }

    class MockProcessor : IWordProcessor
    {
        public int FinishCalled = 0;
        public int WordsProcessed = 0;
        public int OutputsSet = 0;
        public int StartsCalled = 0;
        public List<string> words = new List<string>();

        public void Finish()
        {
            FinishCalled++;
        }

        public void ProcessWord(string word)
        {
            WordsProcessed++;
            words.Add(word);
        }

        public void SetOutput(TextWriter output)
        {
            OutputsSet++;
        }

        public void Start()
        {
            StartsCalled++;
        }
    }

    [TestClass]
    public class WordProcessingAlgorithm_Tests
    {
        [TestMethod]
        public void Run_WithEmptyInput()
        {
           var r = new MockReader(new string[]{ });
           var p = new MockProcessor();

            var alg = new WordProcessingAlgorithm(r, p);
            alg.Run();

            CollectionAssert.AreEqual(new List<string>(), p.words);
            Assert.AreEqual(1, p.FinishCalled);
            Assert.AreEqual(0, p.WordsProcessed);
        }

        [TestMethod]
        public void Run_WithNonEmptyInput()
        {
            string[] words = new string[]{"No,", "I", "am", "your", "father.", "Nooooo!"};
            var r = new MockReader(words);
            var p = new MockProcessor();

            var alg = new WordProcessingAlgorithm(r, p);
            alg.Run();

            CollectionAssert.AreEqual(words, p.words);
            Assert.AreEqual(words.Length, p.WordsProcessed);
            Assert.AreEqual(1, p.FinishCalled);
        }

        [TestMethod]
        public void RunWithoutFinish_WithEmptyInput()
        {
            var r = new MockReader(new string[]{ });
           var p = new MockProcessor();

            var alg = new WordProcessingAlgorithm(r, p);
            alg.RunWithoutFinish();

            CollectionAssert.AreEqual(new List<string>(), p.words);
            Assert.AreEqual(0, p.FinishCalled);
            Assert.AreEqual(0, p.WordsProcessed);
        }

        [TestMethod]
        public void RunWithoutFinish_WithNonEmptyInput()
        {
            string[] words = new string[]{"No,", "I", "am", "your", "father.", "Nooooo!"};
            var r = new MockReader(words);
            var p = new MockProcessor();

            var alg = new WordProcessingAlgorithm(r, p);
            alg.RunWithoutFinish();

            CollectionAssert.AreEqual(words, p.words);
            Assert.AreEqual(words.Length, p.WordsProcessed);
            Assert.AreEqual(0, p.FinishCalled);
        }
    }
}
