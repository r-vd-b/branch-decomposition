using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition
{
    /// <summary>
    /// A fixed size bitset that allows set operations.
    /// </summary>
    /// <remarks>
    /// In general, methods and setters will mutate the bitset, whereas operators will return a new bitset object.
    /// </remarks>
    class BitSet : IEnumerable, IEquatable<BitSet>
    {
        #region Private Variables
        // The internal representation of the bits
        private ulong[] container;
        // The size of the bitset.
        private int size;

        // The size of a ulong (64-bits)
        private const int elementSize = 8 * sizeof(ulong);
        // Bits in the mask are set to one if and only if they are inside within the size of the bitset.
        private readonly ulong mask = ulong.MaxValue;

        // Indicates whether the bitset has been modified since the last call to the Count-property.
        private bool changed = false;
        // The number of bits set to one.
        private int count = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize an empty bitset.
        /// </summary>
        /// <param name="length">The size of the bitset.</param>
        public BitSet(int length)
        {
            this.size = length;
            // The number of ulongs needed to store the bitset.
            this.container = new ulong[(length - 1) / elementSize + 1];

            int remainder = length % elementSize;
            // The mask hides bits outside the scope of the bitset.
            if (remainder != 0)
                this.mask = ulong.MaxValue >> (elementSize - remainder);
        }

        /// <summary>
        /// Initialize a bitset with a singe bit set.
        /// </summary>
        /// <param name="length">The size of the bitset.</param>
        /// <param name="index">The bit that will be set.</param>
        public BitSet(int length, int index) : this(length)
        {
            this[index] = true;
            this.count = 1;
        }

        public BitSet(int length, IEnumerable<int> indices) : this(length)
        {
            foreach (var index in indices)
            {
                this[index] = true;
                this.count++;
            }
        }

        /// <summary>
        /// Construct a new bitset by cloning an existing bitset.
        /// </summary>
        /// <param name="b">The bitset that will be copied.</param>
        public BitSet(BitSet b)
        {
            this.size = b.size;
            this.container = new ulong[b.container.Length];
            Array.Copy(b.container, this.container, b.container.Length);
            this.mask = b.mask;
            this.changed = b.changed;
            this.count = b.count;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets a single element. The setter mutates the bitset.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <returns>A boolean value indicating whether the element is contained in the bitset.</returns>
        public bool this[int index]
        {
            get
            {
                ulong position = 1ul << (index % elementSize);
                return (this.container[index / elementSize] & position) != 0;
            }
            set
            {
                if (value)
                    this.container[index / elementSize] |= (1ul << (index % elementSize));
                else
                    this.container[index / elementSize] &= ulong.MaxValue - (1ul << (index % elementSize));
                changed = true;
            }
        }

        /// <summary>
        /// The size of the bitset.
        /// </summary>
        public int Size
        {
            get { return this.size; }
        }

        /// <summary>
        /// Is the bitset empty?
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                // If the number of elements in the set is known, use this information.
                if (!changed)
                    return this.count == 0;

                // Otherwise, check if each ulong is zero.
                foreach (ulong ul in this.container)
                    if (ul > 0)
                        return false;
                return true;
            }
        }

        /// <summary>
        /// The number of bits set to one in the bitset.
        /// </summary>
        public int Count
        {
            get
            {
                // Return the count if it is still up-to-date.
                if (!changed)
                    return this.count;

                // Otherwise, count the number of ones in each ulong.
                this.count = 0;
                for (int i = 0; i < this.container.Length; i++)
                {
                    // Bitwise counting using sideways addition, see https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel.
                    ulong element = this.container[i];
                    element = element - ((element >> 1) & 0x5555555555555555ul);
                    element = (element & 0x3333333333333333ul) + ((element >> 2) & 0x3333333333333333ul);
                    this.count += (int)(unchecked(((element + (element >> 4)) & 0xF0F0F0F0F0F0F0Ful) * 0x101010101010101ul) >> 56);
                }
                this.changed = false;
                return this.count;
            }
        }
        #endregion

        #region Public Methods
        public override bool Equals(object obj)
        {
            BitSet other = obj as BitSet;
            if (other == null)
                return false;

            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 29;
                for (int i = 0; i < this.container.Length; i++)
                    hash = hash * 486187739 + this.container[i].GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            char[] array = new char[this.size];
            for (int i = 0; i < this.size; i++)
                array[i] = (this[i] ? '1' : '0');

            return new string(array);
        }

        /// <summary>
        /// Mutates this bitset to the intersection of two bitsets.
        /// </summary>
        /// <param name="b">The other bitset.</param>
        /// <returns>This bitset after intersection.</returns>
        public BitSet And(BitSet b)
        {
            this.changed = true;
            for (int i = 0; i < this.container.Length; i++)
                this.container[i] &= b.container[i];
            return this;
        }

        /// <summary>
        /// Mutates this bitset to the union of two bitsets.
        /// </summary>
        /// <param name="b">The other bitset.</param>
        /// <returns>This bitset after union.</returns>
        public BitSet Or(BitSet b)
        {
            this.changed = true;
            for (int i = 0; i < this.container.Length; i++)
                this.container[i] |= b.container[i];
            return this;
        }

        /// <summary>
        /// Mutates this bitset to the symmetric difference (union minus intersection) of two bitsets.
        /// </summary>
        /// <param name="b">The other bitset.</param>
        /// <returns>This bitset after taking the symmetric difference.</returns>
        public BitSet Xor(BitSet b)
        {
            this.changed = true;
            for (int i = 0; i < this.container.Length; i++)
                this.container[i] ^= b.container[i];
            return this;
        }

        /// <summary>
        /// Mutates this bitset to its relative complement (difference) to the other bitset, excluding the elements of this bitset that are in the other bitset as well.
        /// </summary>
        /// <param name="b">The other bitset.</param>
        /// <returns>This bitset after computing the relative complement.</returns>
        public BitSet Exclude(BitSet b)
        {
            this.changed = true;
            for (int i = 0; i < this.container.Length; i++)
                this.container[i] &= ~b.container[i];
            return this;
        }

        /// <summary>
        /// Mutates this bitset to its absolute complement.
        /// </summary>
        /// <returns>This bitset as its absolute complement.</returns>
        public BitSet Not()
        {
            this.count = this.size - this.count;

            for (int i = 0; i < this.container.Length; i++)
                this.container[i] = ~this.container[i];
            // Ensure that bits outside the scope of the bitset are set to zero.
            this.container[this.container.Length - 1] &= this.mask;
            return this;
        }

        /// <summary>
        /// Does this bitset intersect with the other bitset?
        /// </summary>
        /// <param name="b">The other bitset.</param>
        public bool Intersects(BitSet b)
        {
            for (int i = 0; i < this.container.Length; i++)
                if ((this.container[i] & b.container[i]) > 0)
                    return true;
            return false;
        }

        /// <summary>
        /// Is this bitset a superset of the other bitset?
        /// </summary>
        /// <param name="b">The other bitset.</param>
        public bool IsSupersetOf(BitSet b)
        {
            for (int i = 0; i < this.container.Length; i++)
                if ((b.container[i] & ~this.container[i]) > 0)
                    return false;
            return true;
        }

        /// <summary>
        /// Is this bitset a subset of the other bitset?
        /// </summary>
        /// <param name="b">The other bitset.</param>
        public bool IsSubsetOf(BitSet b)
        {
            for (int i = 0; i < this.container.Length; i++)
                if ((this.container[i] & ~b.container[i]) > 0)
                    return false;
            return true;
        }

        /// <summary>
        /// Mutates the bitset to the empty set.
        /// </summary>
        /// <returns>This empty bitset.</returns>
        public BitSet Clear()
        {
            for (int i = 0; i < this.container.Length; i++)
                this.container[i] = 0;
            this.count = 0;
            this.changed = false;
            return this;
        }

        public List<int> ToList()
        {
            List<int> result = new List<int>();
            foreach (int index in this)
                result.Add(index);
            return result;
        }

        public List<T> ToList<T>(IList<T> elements)
        {
            List<T> result = new List<T>();
            foreach (int index in this)
                result.Add(elements[index]);
            return result;
        }

        public List<T> ToList<T>(Func<int, T> selector)
        {
            List<T> result = new List<T>();
            foreach (int index in this)
                result.Add(selector(index));
            return result;
        }
        #endregion

        #region Operator Overloads
        /// <summary>
        /// Constructs the intersection of two bitsets.
        /// </summary>
        /// <param name="b1">The first bitset.</param>
        /// <param name="b2">The second bitset.</param>
        /// <returns>The intersection of the two bitsets.</returns>
        public static BitSet operator &(BitSet b1, BitSet b2)
        {
            BitSet result = new BitSet(b1.size);
            for (int i = 0; i < b1.container.Length; i++)
                result.container[i] = b1.container[i] & b2.container[i];
            result.changed = true;
            return result;
        }

        /// <summary>
        /// Constructs the union of the two bitsets.
        /// </summary>
        /// <param name="b1">The first bitset.</param>
        /// <param name="b2">The second bitset.</param>
        /// <returns>The union of the two bitsets.</returns>
        public static BitSet operator |(BitSet b1, BitSet b2)
        {
            BitSet result = new BitSet(b1.size);
            for (int i = 0; i < b1.container.Length; i++)
                result.container[i] = b1.container[i] | b2.container[i];
            result.changed = true;
            return result;
        }

        /// <summary>
        /// Constructs the symmetric difference (union minus intersection) of the two bitsets.
        /// </summary>
        /// <param name="b1">The first bitset.</param>
        /// <param name="b2">The second bitset.</param>
        /// <returns>The symmetric difference of the two bitsets.</returns>
        public static BitSet operator ^(BitSet b1, BitSet b2)
        {
            BitSet result = new BitSet(b1.size);
            for (int i = 0; i < b1.container.Length; i++)
                result.container[i] = b1.container[i] ^ b2.container[i];
            result.changed = true;
            return result;
        }

        /// <summary>
        /// Constructs the relative complement (difference) of the first bitset to the second bitset.
        /// </summary>
        /// <param name="baseSet">The base set of elements.</param>
        /// <param name="toBeExcluded">The elements to be excluded.</param>
        /// <returns>The relative complement of the two bitsets.</returns>
        public static BitSet operator -(BitSet baseSet, BitSet toBeExcluded)
        {
            BitSet result = new BitSet(baseSet.size);
            for (int i = 0; i < toBeExcluded.container.Length; i++)
                result.container[i] = baseSet.container[i] & ~toBeExcluded.container[i];
            result.changed = true;
            return result;
        }

        /// <summary>
        /// Constructs the absolute complement of the bitset.
        /// </summary>
        /// <param name="b">The bitset of which we will construct the complement.</param>
        /// <returns>The complement.</returns>
        public static BitSet operator ~(BitSet b)
        {
            BitSet result = new BitSet(b.size);
            for (int i = 0; i < b.container.Length; i++)
                result.container[i] = ~b.container[i];
            result.container[result.container.Length - 1] &= result.mask;
            result.changed = true;
            return result;
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < this.container.Length; i++)
            {
                int index = elementSize * i;
                for (ulong element = this.container[i]; element != 0; element = element >> 1)
                {
                    if ((element & 1ul) == 1ul)
                        yield return index;
                    index++;
                }
            }
        }
        #endregion

        #region IEquatable<BitSet> Members
        public bool Equals(BitSet other)
        {
            if (other == null || this.size != other.size)
                return false;

            for (int i = 0; i < this.container.Length; i++)
                if (this.container[i] != other.container[i])
                    return false;

            return true;
        }
        #endregion
    }
}
