using System;
using System.Dynamic;
using Cuni.Arithmetics.FixedPoint;
using NUnit.Framework;

namespace Tests
{
    public class FixedTests_Q24_8 : FixedTests<Q24_8> { }
    public class FixedTests_Q16_16 : FixedTests<Q16_16> { }
    public class FixedTests_Q8_24 : FixedTests<Q8_24> { }
    
    public abstract class FixedTests<T> where T : Q, new()
    {
        private static readonly T singleton = new T();
        private static readonly int maxInt = (int) Math.Pow(2, singleton.PreBits - 1);
        private static readonly int minInt = -maxInt - 1;
        private static readonly double precision = Math.Pow(2, -new T().PostBits);

        
        [Test]
        public void Fixed_PositiveIntegerAssignment() 
        {
            for (int i = 0; i < maxInt; i++)
            {
                Assert.AreEqual(i, (double) new Fixed<T>(i));
            }
        }

        [Test]
        public void Fixed_NegativeIntegerAssignment()
        {
            for (int i = 0; i > minInt; i--)
            {
                Assert.AreEqual(i, (double) new Fixed<T>(i));
            }
        }

        [Test]
        public void Fixed_AddingPositives()
        {
            int i = 2;
            int j = 3;
            Assert.AreEqual(i+j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i+j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = 82;
            j = 27;
            Assert.AreEqual(i+j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i+j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
        }

        
        [Test]
        public void Fixed_AddingNegatives()
        {
            int i = -3;
            int j = -4;
            Assert.AreEqual(i+j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i+j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = -73;
            j = -39;
            Assert.AreEqual(i+j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i+j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_AddingMixed()
        {
            int i = 5;
            int j = -8;
            Assert.AreEqual(i+j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i+j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = 112;
            j = -35;
            Assert.AreEqual(i+j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i+j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = 45;
            j = 0;
            Assert.AreEqual(i+j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i+j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = -26;
            Assert.AreEqual(i+j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i+j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = 0;
            Assert.AreEqual(i+j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i+j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_SubbingPositive()
        {
            int i = 85;
            int j = 40;
            Assert.AreEqual(i-j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j-i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
            i = 37;
            j = 63;
            Assert.AreEqual(i-j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j-i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_SubbingNegative()
        {
            int i = -5;
            int j = -12;
            Assert.AreEqual(i-j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j-i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
            i = -15;
            j = -106;
            Assert.AreEqual(i-j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j-i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_SubbingMixed()
        {
            int i = 39;
            int j = -73;
            Assert.AreEqual(i-j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j-i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
            i = 26;
            j = 0;
            Assert.AreEqual(i-j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j-i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
            i = -75;
            Assert.AreEqual(i-j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j-i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_MultiplyPositive()
        {
            int i = 8;
            int j = 12;
            Assert.AreEqual(i*j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i*j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_MultiplyNegative()
        {
            int i = -13;
            int j = -9;
            Assert.AreEqual(i*j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i*j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_MultiplyMixed()
        {
            int i = 19;
            int j = -4;
            Assert.AreEqual(i*j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i*j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
            i = 95;
            j = 0;
            Assert.AreEqual(i*j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i*j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
            i = -78;
            Assert.AreEqual(i*j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i*j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
            i = 0;
            Assert.AreEqual(i*j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i*j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
        }

        
        [Test]
        public void Fixed_DividePositive()
        {
            int i = 119;
            int j = 36;
            
            var f = new Fixed<T>(i).Divide(new Fixed<T>(j));
            var g = new Fixed<T>(j).Divide(new Fixed<T>(i));
            Assert.Less(Math.Abs((double) i/j - (double) f), precision);
            Assert.Less(Math.Abs((double) j/i - (double) g), precision);
        }

        [Test]
        public void Fixed_DivideNegative()
        {
            int i = -125;
            int j = -86;
            var f = new Fixed<T>(i).Divide(new Fixed<T>(j));
            var g = new Fixed<T>(j).Divide(new Fixed<T>(i));
            Assert.Less(Math.Abs((double) i/j - (double) f), precision);
            Assert.Less(Math.Abs((double) j/i - (double) g), precision);
        }

        [Test]
        public void Fixed_DivideMixed()
        {
            int i = 47;
            int j = -10;
            var f = new Fixed<T>(i).Divide(new Fixed<T>(j));
            var g = new Fixed<T>(j).Divide(new Fixed<T>(i));
            Assert.Less(Math.Abs((double) i/j - (double) f), precision);
            Assert.Less(Math.Abs((double) j/i - (double) g), precision);
            i = 0;
            Assert.AreEqual(0, (double) new Fixed<T>(i).Divide(new Fixed<T>(j)));
            Assert.Catch(typeof(DivideByZeroException), delegate { new Fixed<T>(j).Divide(new Fixed<T>(i));});
        }

        [Test]
        public void Fixed_ToString()
        {
            var f = new Fixed<T>(67);
            Assert.AreEqual(((double) f).ToString(), f.ToString());
            f = f.Add(new Fixed<T>(48));
            Assert.AreEqual(((double) f).ToString(), f.ToString());
            f = f.Subtract(new Fixed<T>(85));
            Assert.AreEqual(((double) f).ToString(), f.ToString());
            f = f.Multiply(new Fixed<T>(6));
            Assert.AreEqual(((double) f).ToString(), f.ToString());
            f = f.Divide(new Fixed<T>(7));
            Assert.AreEqual(((double) f).ToString(), f.ToString());
            
            f = new Fixed<T>(-95);
            Assert.AreEqual(((double) f).ToString(), f.ToString());
            f = f.Divide(new Fixed<T>(11));
            Assert.AreEqual(((double) f).ToString(), f.ToString());
            f = new Fixed<T>(0);
            Assert.AreEqual("0", f.ToString());
        }
    }
    
}