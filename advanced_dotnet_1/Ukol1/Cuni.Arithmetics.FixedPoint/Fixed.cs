using System;

namespace Cuni.Arithmetics.FixedPoint
{
    /// <summary>
    /// Represents a signed number in fixed point notation with 32 bits of precision
    /// </summary>
    /// <typeparam name="T">
    /// Describes the position of the decimal point - must be a descendant of Q
    /// </typeparam>
    public struct Fixed<T> : IComparable<Fixed<T>>, IEquatable<Fixed<T>> where T : Q, new()
    {
        private readonly int value;                // actual bit value of the fixed point number
        private static readonly T t = new T();     // static singleton of the specified type to get decimal point position

        /// <summary>
        /// Used to directly set the inner value of Fixed
        /// </summary>
        private struct FpValue
        {
            internal int bits;
        }
        
        /// <summary>
        /// Creates a fixed point number equal to <paramref name="value"/>
        /// </summary>
        public Fixed(int value)
        {
            this.value = value << t.PostBits;
        }

        private Fixed(FpValue inner) => value = inner.bits;
    
        /// <summary>
        /// Adds two Fixed numbers of the same type together
        /// </summary>
        /// <param name="other">
        /// Number to be added to caller
        /// </param>
        /// <returns>
        /// New Fixed<T> instance with the added value
        /// </returns>
        public Fixed<T> Add(Fixed<T> other)
        {
            return new Fixed<T>(new FpValue{bits = value + other.value});
        }
    
        
        /// <summary>
        /// Subtracts one Fixed number from another
        /// </summary>
        /// <param name="other">
        /// Number to be subtracted from caller
        /// </param>
        /// <returns>
        /// New Fixed<T> instance with the subtracted value
        /// </returns>
        public Fixed<T> Subtract(Fixed<T> other)
        {
            return new Fixed<T>(new FpValue{bits = value - other.value});
        }
    
        /// <summary>
        /// Multiplies two Fixed numbers of the same type
        /// </summary>
        /// <param name="other">
        /// Number to multiply caller by
        /// </param>
        /// <returns>
        /// New Fixed<T> instance with the multiplied value
        /// </returns>
        public Fixed<T> Multiply(Fixed<T> other)
        {
            long res = (value * (long) other.value) >> t.PostBits;
            return new Fixed<T>(new FpValue{bits = (int) res});
        }
        
        /// <summary>
        /// Divides one Fixed number by another
        /// </summary>
        /// <param name="other">
        /// Number to divide caller by
        /// </param>
        /// <returns>
        /// New Fixed<T> instance with the divided value
        /// </returns>
        public Fixed<T> Divide(Fixed<T> other)
        {
            long res = ((long) value << t.PostBits) / other.value;
            return new Fixed<T>(new FpValue{bits = (int) res});
        }

        /// <summary>
        /// Converts to a Fixed number of different precision 
        /// </summary>
        /// <typeparam name="R">
        /// Desired precision type
        /// </typeparam>
        /// <returns>
        /// New Fixed instance of the specified type
        /// </returns>
        public Fixed<R> ConvertTo<R>() where R : Q, new()
        {
            var r = new R();
            var bits = r.PostBits > t.PostBits ? value << r.PostBits - t.PostBits : value >> t.PostBits - r.PostBits;
            return new Fixed<R>(new Fixed<R>.FpValue{ bits = bits });
        }

#region casts and operators
        // Explicit conversion from Fixed to double
        public static explicit operator double(Fixed<T> f) => f.value / (double) (1 << t.PostBits);
        // Implicit conversion from int to Fixed
        public static implicit operator Fixed<T>(int i) => new Fixed<T>(i);
        
        // All operators also work with ints thanks to the implicit conversion above
        // Comparison operators
        public static bool operator ==(Fixed<T> a, Fixed<T> b) => a.value == b.value;
        public static bool operator !=(Fixed<T> a, Fixed<T> b) => !(a == b);
        public static bool operator <(Fixed<T> a, Fixed<T> b) => a.value < b.value;
        public static bool operator >(Fixed<T> a, Fixed<T> b) => a.value > b.value;
        public static bool operator >=(Fixed<T> a, Fixed<T> b) => a > b || a == b;
        public static bool operator <=(Fixed<T> a, Fixed<T> b) => a < b || a == b;
        
        // Arithmetic operators
        public static Fixed<T> operator +(Fixed<T> a, Fixed<T> b) => a.Add(b);
        public static Fixed<T> operator -(Fixed<T> a, Fixed<T> b) => a.Subtract(b);
        public static Fixed<T> operator *(Fixed<T> a, Fixed<T> b) => a.Multiply(b);
        public static Fixed<T> operator /(Fixed<T> a, Fixed<T> b) => a.Divide(b);
#endregion

#region basic overrides and interface implementations
        public override string ToString()
        {
            return ((double) this).ToString();
        }
        
        public int CompareTo(Fixed<T> other)
        {
            return value.CompareTo(other.value);
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            return value == ((Fixed<T>) obj).value;
        }

        public bool Equals(Fixed<T> other)
        {
            return value == other.value;
        }

        public override int GetHashCode()
        {
            return value;
        }
#endregion 
    }
}