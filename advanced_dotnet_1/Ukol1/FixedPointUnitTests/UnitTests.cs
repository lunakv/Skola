using System;
using Cuni.Arithmetics.FixedPoint;
using NUnit.Framework;

namespace Tests
{
    public class FixedTests_Q24_8 : FixedTests<Q24_8>
    {
        public override double Precision => Math.Pow(2, -8);
    }

    public class FixedTests_Q16_16 : FixedTests<Q16_16>
    {
        public override double Precision => Math.Pow(2, -16);
    }

    public class FixedTests_Q8_24 : FixedTests<Q8_24>
    {
        public override double Precision => Math.Pow(2, -24);
    }

    public abstract class FixedTests<T> where T : Q, new()
    {
        public abstract double Precision { get; }

        [Test]
        public void Fixed_Assignments()
        {
            Assert.AreEqual(73.0, (double) new Fixed<T>(73));
            Assert.AreEqual(0.0, (double) new Fixed<T>(0));
            Assert.AreEqual(-46.0, (double) new Fixed<T>(-46));
        }

        [Test]
        public void Fixed_AddingPositives()
        {
            int i = 2;
            int j = 3;
            Assert.AreEqual(i + j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i + j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = 82;
            j = 27;
            Assert.AreEqual(i + j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i + j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_AddingNegatives()
        {
            int i = -3;
            int j = -4;
            Assert.AreEqual(i + j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i + j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = -73;
            j = -39;
            Assert.AreEqual(i + j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i + j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_AddingMixed()
        {
            int i = 5;
            int j = -8;
            Assert.AreEqual(i + j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i + j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = 112;
            j = -35;
            Assert.AreEqual(i + j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i + j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = 45;
            j = 0;
            Assert.AreEqual(i + j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i + j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = -26;
            Assert.AreEqual(i + j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i + j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
            i = 0;
            Assert.AreEqual(i + j, (double) new Fixed<T>(i).Add(new Fixed<T>(j)));
            Assert.AreEqual(i + j, (double) new Fixed<T>(j).Add(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_SubbingPositive()
        {
            int i = 85;
            int j = 40;
            Assert.AreEqual(i - j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j - i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
            i = 37;
            j = 63;
            Assert.AreEqual(i - j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j - i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_SubbingNegative()
        {
            int i = -5;
            int j = -12;
            Assert.AreEqual(i - j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j - i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
            i = -15;
            j = -106;
            Assert.AreEqual(i - j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j - i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_SubbingMixed()
        {
            int i = 39;
            int j = -73;
            Assert.AreEqual(i - j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j - i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
            i = 26;
            j = 0;
            Assert.AreEqual(i - j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j - i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
            i = -75;
            Assert.AreEqual(i - j, (double) new Fixed<T>(i).Subtract(new Fixed<T>(j)));
            Assert.AreEqual(j - i, (double) new Fixed<T>(j).Subtract(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_MultiplyPositive()
        {
            int i = 8;
            int j = 12;
            Assert.AreEqual(i * j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i * j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_MultiplyNegative()
        {
            int i = -13;
            int j = -9;
            Assert.AreEqual(i * j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i * j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
        }

        [Test]
        public void Fixed_MultiplyMixed()
        {
            int i = 19;
            int j = -4;
            Assert.AreEqual(i * j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i * j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
            i = 95;
            j = 0;
            Assert.AreEqual(i * j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i * j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
            i = -78;
            Assert.AreEqual(i * j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i * j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
            i = 0;
            Assert.AreEqual(i * j, (double) new Fixed<T>(i).Multiply(new Fixed<T>(j)));
            Assert.AreEqual(i * j, (double) new Fixed<T>(j).Multiply(new Fixed<T>(i)));
        }


        [Test]
        public void Fixed_DividePositive()
        {
            int i = 119;
            int j = 36;

            var f = new Fixed<T>(i).Divide(new Fixed<T>(j));
            var g = new Fixed<T>(j).Divide(new Fixed<T>(i));
            Assert.Less(Math.Abs((double) i / j - (double) f), Precision);
            Assert.Less(Math.Abs((double) j / i - (double) g), Precision);
        }

        [Test]
        public void Fixed_DivideNegative()
        {
            int i = -125;
            int j = -86;
            var f = new Fixed<T>(i).Divide(new Fixed<T>(j));
            var g = new Fixed<T>(j).Divide(new Fixed<T>(i));
            Assert.Less(Math.Abs((double) i / j - (double) f), Precision);
            Assert.Less(Math.Abs((double) j / i - (double) g), Precision);
        }

        [Test]
        public void Fixed_DivideMixed()
        {
            int i = 47;
            int j = -10;
            var f = new Fixed<T>(i).Divide(new Fixed<T>(j));
            var g = new Fixed<T>(j).Divide(new Fixed<T>(i));
            Assert.Less(Math.Abs((double) i / j - (double) f), Precision);
            Assert.Less(Math.Abs((double) j / i - (double) g), Precision);
            i = 0;
            Assert.AreEqual(0, (double) new Fixed<T>(i).Divide(new Fixed<T>(j)));
            Assert.Catch(typeof(DivideByZeroException), delegate { new Fixed<T>(j).Divide(new Fixed<T>(i)); });
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

        [Test]
        public void Fixed_intAssignment()
        {
            Fixed<T> f = 63;
            Assert.AreEqual(new Fixed<T>(63), f);
            f = -15;
            Assert.AreEqual(new Fixed<T>(-15), f);
        }

        [Test]
        public void Fixed_OperatorAddition()
        {
            var f = new Fixed<T>(51);
            var g = new Fixed<T>(13);
            Assert.AreEqual(new Fixed<T>(64), f + g);
            Assert.AreEqual(new Fixed<T>(64), g + f);
        }

        [Test]
        public void Fixed_OperatorSubtraction()
        {
            var f = new Fixed<T>(75);
            var g = new Fixed<T>(42);
            Assert.AreEqual(new Fixed<T>(75 - 42), f - g);
        }

        [Test]
        public void Fixed_OperatorMultiplication()
        {
            var f = new Fixed<T>(26);
            var g = new Fixed<T>(4);
            Assert.AreEqual(new Fixed<T>(26 * 4), f * g);
            Assert.AreEqual(new Fixed<T>(26 * 4), g * f);
        }

        [Test]
        public void Fixed_OperatorDivision()
        {
            var f = new Fixed<T>(119);
            var g = new Fixed<T>(7);
            Assert.AreEqual(f.Divide(g), f / g);
        }

        [Test]
        public void Fixed_intOperators()
        {
            var f = new Fixed<T>(27);
            int i = -3;
            Assert.AreEqual(f + new Fixed<T>(i), f + i);
            Assert.AreEqual(f - new Fixed<T>(i), f - i);
            Assert.AreEqual(new Fixed<T>(i) * f, f * i);
            Assert.Less(Math.Abs((double) (new Fixed<T>(i) / f - i / f)), Precision);
        }

        [Test]
        public void Fixed_OperatorComparisons()
        {
            var f = new Fixed<T>(49);
            var g = new Fixed<T>(-4);
            Assert.True(f > g);
            Assert.True(f >= g);
            Assert.True(f != g);
            Assert.False(f == g);
            Assert.False(f <= g);
            Assert.False(f < g);
        }

        [Test]
        public void Fixed_CompareTo()
        {
            Assert.Negative(new Fixed<T>(-10).CompareTo(new Fixed<T>(15)));
            Assert.Zero(new Fixed<T>(30).CompareTo(new Fixed<T>(30)));
            Assert.Positive(new Fixed<T>(88).CompareTo(new Fixed<T>(36)));
        }

        [Test]
        public void Fixed_Equals()
        {
            Assert.False(new Fixed<T>(15).Equals(new Fixed<T>(19)));
            Assert.True(new Fixed<T>(20).Equals(new Fixed<T>(20)));
            Assert.False(new Fixed<T>(35).Equals(35.0));
            Assert.True(new Fixed<T>(19).Equals(19));
        }

        [Test]
        public void Fixed_ConvertPrecision()
        {
            var f1 = new Fixed<T>(112);
            var f2 = f1.ConvertTo<Q8_24>();
            var f3 = f1.ConvertTo<Q16_16>();
            var f4 = f1.ConvertTo<Q24_8>();
            Assert.AreEqual((double) f1, (double) f2);
            Assert.AreEqual((double) f1, (double) f3);
            Assert.AreEqual((double) f1, (double) f4);
        }
}
    
}