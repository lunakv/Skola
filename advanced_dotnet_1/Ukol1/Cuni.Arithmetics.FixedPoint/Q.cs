namespace Cuni.Arithmetics.FixedPoint
{
    /// <summary>
    /// Abstract class specifying the position of the floating point
    /// </summary>
    public abstract class Q
    {
        internal abstract int PostBits { get; } // number of bits after the decimal point
    }
    public class Q24_8 : Q
    {
        internal override int PostBits => 8;
    }

    public class Q16_16 : Q
    {
        internal override int PostBits => 16;
    }

    public class Q8_24 : Q
    {
        internal override int PostBits => 24;
    }
}