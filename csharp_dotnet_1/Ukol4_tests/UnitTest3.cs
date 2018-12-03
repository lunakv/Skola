using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ukol4;

namespace Ukol4_tests
{
    public class MockOutput : TextWriter
    {
        public string Output = "";
        public bool isOpened;
        public override Encoding Encoding => throw new NotImplementedException();

        public MockOutput()
        {
            isOpened = true;
        }

        public override void Write(string value)
        {
            Output += value;
        }

        public override void Write(char value)
        {
            Output += value;
        }

        public override void WriteLine(string value)
        {
            this.Write(value);
            this.Write("\n");
        }

        public override void WriteLine(char value)
        {
            this.Write(value);
            this.Write("\n");
        }

        public override void WriteLine()
        {
            this.Write("\n");
        }
        public override void Close()
        {
            isOpened = false;
        }
    }

    [TestClass]
    public class BlockAligner_tests
    {
        string SuperLongWord = "ThisWordIsVeryLongItIsSoLongItWillNotFitOnOneLineEvenIfItTriesToDoThat";
        void ProcessWord_OneLargeWord(int size)
        {
            string word = SuperLongWord;
            if (word.Length < size) word += word;
            var mO = new MockOutput();

            var p = new BlockAligner(size, mO);
            p.ProcessWord(word);
            p.Finish();

            Assert.AreEqual(word + "\n", mO.Output);
        }

        [TestMethod]
        public void ProcessWord17_OneLargeWord()
        {
            ProcessWord_OneLargeWord(17);
        }

        [TestMethod]
        public void ProcessWord40_OneLargeWord()
        {
            ProcessWord_OneLargeWord(40);
        }

        void ProcessWord_OneShortWord(int size)
        {
            string word = "ahoj";
            var mO = new MockOutput();
            string line = word;

            var p = new BlockAligner(size, mO);
            p.ProcessWord(word);
            p.ProcessWord(SuperLongWord);
            p.Finish();

            Assert.AreEqual(line + "\n" + SuperLongWord + "\n", mO.Output);
        }

        [TestMethod]
        public void ProcessWord17_OneShortWord()
        {
            ProcessWord_OneShortWord(17);
        }

        [TestMethod]
        public void ProcessWord40_OneShortWord()
        {
            ProcessWord_OneShortWord(40);
        }

        public void ProcessWord_TwoWords(int size)
        {
            string word1 = "Hello";
            string word2 = "there";
            string line = word1;
            for (int i = 0; i < size - 10; i++)
                line += " ";
            line += word2;

            var mO = new MockOutput();

            var p = new BlockAligner(size, mO);
            p.ProcessWord(word1);
            p.ProcessWord(word2);
            p.ProcessWord(SuperLongWord);
            p.Finish();

            Assert.AreEqual(line + "\n" + SuperLongWord + "\n", mO.Output);
        }

        [TestMethod]
        public void ProcessWord17_TwoWords()
        {
            ProcessWord_TwoWords(17);
        }

        [TestMethod]
        public void ProcessWord40_TwoWords()
        {
            ProcessWord_TwoWords(40);
        }

        [TestMethod]
        public void ProcessWord17_ThreeWordsEvenSpaces()
        {
            string word1 = "I'm", word2 = "an", word3 = "orange";
            string line = "I'm   an   orange";
            var mO = new MockOutput();

            var p = new BlockAligner(17, mO);
            p.ProcessWord(word1);
            p.ProcessWord(word2);
            p.ProcessWord(word3);
            p.ProcessWord(SuperLongWord);
            p.Finish();

            Assert.AreEqual(line + "\n" + SuperLongWord + "\n", mO.Output);
        }

        [TestMethod]
        public void ProcessWord40_ThreeWordsEvenSpaces()
        {
            string word1 = "I'm", word2 = "an", word3 = "orange";
            string line = "I'm               an              orange";
            var mO = new MockOutput();

            var p = new BlockAligner(40, mO);
            p.ProcessWord(word1);
            p.ProcessWord(word2);
            p.ProcessWord(word3);
            p.ProcessWord(SuperLongWord);
            p.Finish();

            Assert.AreEqual(line + "\n" + SuperLongWord + "\n", mO.Output);
        }

        [TestMethod]
        public void ProcessWord17_ThreeWordsUnevenSpaces()
        {
            string word1 = "I'm", word2 = "a", word3 = "potato";
            string line = "I'm    a   potato";
            var mO = new MockOutput();

            var p = new BlockAligner(17, mO);
            p.ProcessWord(word1);
            p.ProcessWord(word2);
            p.ProcessWord(word3);
            p.ProcessWord(SuperLongWord);
            p.Finish();

            Assert.AreEqual(line + "\n" + SuperLongWord + "\n", mO.Output);
        }

        [TestMethod]
        public void ProcessWord40_ThreeWordsUnevenSpaces()
        {
            string word1 = "I'm", word2 = "an", word3 = "orange";
            string line = "I'm               an              orange";
            var mO = new MockOutput();

            var p = new BlockAligner(40, mO);
            p.ProcessWord(word1);
            p.ProcessWord(word2);
            p.ProcessWord(word3);
            p.ProcessWord(SuperLongWord);
            p.Finish();

            Assert.AreEqual(line + "\n" + SuperLongWord + "\n", mO.Output);
        }

        [TestMethod]
        public void ProcessWord17_MoreWordsThanFitOnOneLine()
        {
            string word1 = "Lorem", word2 = "ipsum", word3 = "dolor", word4 = "sit", word5 = "amet.";
            string line1 = "Lorem ipsum dolor";
            string line2 = "sit amet.";
            string[] words = new []{word1, word2, word3, word4, word5};
            var mO = new MockOutput();

            var p = new BlockAligner(17, mO);
            foreach (var word in words)
            {
                p.ProcessWord(word);
            }
            p.Finish();

            Assert.AreEqual(line1 + "\n" + line2 + "\n", mO.Output);
        }

        [TestMethod]
        public void ProcessWord17_TwoParagraphsSeparatedByOneEmptyLine()
        {
            string word1 = "Lorem", word2 = "ipsum", word3 = "dolor", word4 = "sit", word5 = "amet.";
            string line1 = "Lorem ipsum dolor";
            string line2 = "sit amet.";
            string result = line1 + "\n" + line2 + "\n\n" + line1 + "\n" + line2 + "\n";
            string[] words = new []{word1, word2, word3, word4, word5};
            var mO = new MockOutput();

            var p = new BlockAligner(17, mO);
            foreach (var word in words)
            {
                p.ProcessWord(word);
            }
            p.ProcessWord("\n");
            p.ProcessWord("\n");
            foreach (var word in words)
            {
                p.ProcessWord(word);
            }

            p.Finish();

            Assert.AreEqual(result, mO.Output);
        }

        [TestMethod]
        public void ProcessWord17_TwoPragraphsSeparatedByMultipleEmptylines()
        {
            string word1 = "Lorem", word2 = "ipsum", word3 = "dolor", word4 = "sit", word5 = "amet.";
            string line1 = "Lorem ipsum dolor";
            string line2 = "sit amet.";
            string result = line1 + "\n" + line2 + "\n\n" + line1 + "\n" + line2 + "\n";
            string[] words = new []{word1, word2, word3, word4, word5};
            var mO = new MockOutput();

            var p = new BlockAligner(17, mO);
            foreach (var word in words)
            {
                p.ProcessWord(word);
            }
            p.ProcessWord("\n");
            p.ProcessWord("\n");
            p.ProcessWord("\n");
            p.ProcessWord("\n");
            foreach (var word in words)
            {
                p.ProcessWord(word);
            }

            p.Finish();

            Assert.AreEqual(result, mO.Output);
        }

        [TestMethod]
        public void ProcessWord17_MoreInpuptLinesButTheSameParagraph()
        {
            string word1 = "Lorem", word2 = "ipsum", word3 = "dolor", word4 = "sit", word5 = "amet.";
            string line1 = "Lorem ipsum dolor";
            string line2 = "sit  amet.  Lorem";
            string line3 = "ipsum  dolor  sit";
            string line4 = "amet.";
            string result = line1 + "\n" + line2 + "\n" + line3 + "\n" + line4 + "\n";
            string[] words = new []{word1, word2, word3, word4, word5};
            var mO = new MockOutput();

            var p = new BlockAligner(17, mO);
            foreach (var word in words)
            {
                p.ProcessWord(word);
            }
            p.ProcessWord("\n");
            foreach (var word in words)
            {
                p.ProcessWord(word);
            }

            p.Finish();

            Assert.AreEqual(result, mO.Output);
        }

        [TestMethod]
        public void ProcessWord17_OneLineWithHighlitedSpaces()
        {
            string[] words = new []{"This", "is", "a", "line"};
            string result = "This.is.a.line<-\n";
            var mO = new MockOutput();

            var p = new BlockAligner(17, mO);
            p.HighlightSpaces = true;

            foreach (var word in words)
            {
                p.ProcessWord(word);
            }
            p.Finish();

            Assert.AreEqual(result, mO.Output);
        }

        [TestMethod]
        public void ProcessWord17_OneParagraphWithHighlightedSpaces()
        {
            string[] words = new []{"This", "is", "a", "single", "paragraph", "to", "align"};
            string line1 = "This..is.a.single<-\n";
            string line2 = "paragraph......to<-\n";
            string line3 = "align<-\n";
            string result = line1 + line2 + line3;
            var mO = new MockOutput();

            var p = new BlockAligner(17, mO);
            p.HighlightSpaces = true;

            foreach (var word in words)
            {
                p.ProcessWord(word);
            }
            p.Finish();

            Assert.AreEqual(result, mO.Output);
        }

        [TestMethod]
        public void ProcessWord17_MultipleParagraphsWithHighlightedSpaces()
        {
            string[] words = new []{"This", "is", "a", "single", "paragraph", "to", "align"};            
            string[] words2 = new []{"This", "is", "another", "such", "paragraph"};
            string line1 = "This..is.a.single<-\n";
            string line2 = "paragraph......to<-\n";
            string line3 = "align<-\n";
            string line4 = "<-\n";
            string line5 = "This..is..another<-\n";
            string line6 = "such.paragraph<-\n";
            string result = line1 + line2 + line3 + line4 + line5 + line6;
            var mO = new MockOutput();

            var p = new BlockAligner(17, mO);
            p.HighlightSpaces = true;

            foreach (var word in words)
            {
                p.ProcessWord(word);
            }
            p.ProcessWord("\n");
            p.ProcessWord("\n");
            foreach (var word in words2)
            {
                p.ProcessWord(word);
            }
            p.Finish();

            Assert.AreEqual(result, mO.Output);
        }
                
    }
}
