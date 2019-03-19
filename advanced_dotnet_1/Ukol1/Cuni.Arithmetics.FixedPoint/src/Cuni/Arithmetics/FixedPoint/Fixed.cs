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
        private int value;                     // actual bit value of the fixed point number
        private static readonly T t = new T(); // static singleton of the specified type to get decimal point position
    
        /// <summary>
        /// Creates a fixed point number equal to <paramref name="value"/>
        /// </summary>
        public Fixed(int value)
        {
            this.value = value << t.PostBits;
        }
    
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
            return new Fixed<T> {value = this.value + other.value};
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
            return new Fixed<T> {value = this.value - other.value};
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
            // arguments cast to long to maintain precision
            long l1 = this.value;
            long l2 = other.value;
            long res = (l1 * l2) >> t.PostBits;
            return new Fixed<T> {value = (int) res};
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
            // arguments cast to long to maintain precision
            long l1 = this.value;
            long l2 = other.value;
            long res = l1 << t.PostBits;
            res = res / l2;
            return new Fixed<T> {value = (int) res};
        }
    
#region overriddes and interface implementations
        public override string ToString()
        {
            return ((double) this).ToString();
        }
        
        public int CompareTo(Fixed<T> other)
        {
            // any object by definition compares greater than null 
            if (ReferenceEquals(null, other)) return 1;
            return value.CompareTo(other.value);
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            return value == ((Fixed<T>) obj).value;
        }

        public bool Equals(Fixed<T> other)
        {
            if (other == null) return false;
            return value == other.value;
        }

        public override int GetHashCode()
        {
            // value is never modified after assignment - can be used as a hash code
            return value;
        }

        public static bool operator ==(Fixed<T> a, Fixed<T> b)
        {
            // null comparison results in false with everything except null
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.value == b.value;
        }
        
        public static bool operator <(Fixed<T> a, Fixed<T> b)
        {
            // null comparison results in false with everything except null
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.value < b.value;
        }

        public static bool operator >(Fixed<T> a, Fixed<T> b)
        {
            // null comparison results in false with everything except null
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.value > b.value;
        }
        
        public static bool operator !=(Fixed<T> a, Fixed<T> b) => !(a == b);
        public static bool operator >=(Fixed<T> a, Fixed<T> b) => a > b || a == b;
        public static bool operator <=(Fixed<T> a, Fixed<T> b) => a < b || a == b;
        
        public static Fixed<T> operator +(Fixed<T> a, Fixed<T> b) => a.Add(b);
        public static Fixed<T> operator -(Fixed<T> a, Fixed<T> b) => a.Subtract(b);
        public static Fixed<T> operator *(Fixed<T> a, Fixed<T> b) => a.Multiply(b);
        public static Fixed<T> operator /(Fixed<T> a, Fixed<T> b) => a.Divide(b);

        public static explicit operator double(Fixed<T> f)
        {
                         // Integer part of the number 
            double ret = f.value >> t.PostBits;
                         // Fractional part of the number - cast to uint to always zero out top bits
            uint frac = ((uint) f.value << t.PreBits) >> t.PreBits;
            ret += frac * Math.Pow(2, -t.PostBits);
            return ret;
        }
#endregion
    }
}