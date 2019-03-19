namespace Cuni.Arithmetics.FixedPoint
{
    /// <summary>
    /// Abstract class specifying the position of the floating point
    /// </summary>
    public abstract class Q
    {
        public abstract int PreBits { get; } // number of bits before the decimal point
        public abstract int PostBits { get; } // number of bits after the decimal point
    }
    public class Q24_8 : Q
    {
        public override int PreBits => 24;
        public override int PostBits => 8;
    }

    public class Q16_16 : Q
    {
        public override int PreBits => 16;
        public override int PostBits => 16;
    }

    public class Q8_24 : Q
    {
        public override int PreBits => 8;
        public override int PostBits => 24;
    }
}