﻿using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Lidgren.Network
{
    /// <summary>
    /// Mutable array of bits.
    /// </summary>
    public readonly struct NetBitArray : IEquatable<NetBitArray>
    {
        public const int BitsPerElement = sizeof(uint) * 8;

        private readonly uint[]? _data;

        /// <summary>
        /// Gets the total number of stored bits.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the number of bits set to one.
        /// </summary>
        public int PopCount
        {
            get
            {
                int sum = 0;
                Span<uint> slice = _data.AsSpan(0, DivByESize(Length));
                for (int i = 0; i < slice.Length; i++)
                {
                    sum += BitOperations.PopCount(slice[i]);
                }

                int rem = NetUtility.PowOf2Mod(Length, BitsPerElement);
                if (rem != 0)
                {
                    sum += BitOperations.PopCount(_data![^1] & ~(uint.MaxValue << rem));
                }
                return sum;
            }
        }

        /// <summary>
        /// Gets whether all bits are set to zero.
        /// </summary>
        public bool IsZero
        {
            get
            {
                Span<uint> slice = _data.AsSpan(0, DivByESize(Length));
                for (int i = 0; i < slice.Length; i++)
                {
                    if (slice[i] != 0)
                        return false;
                }

                int rem = NetUtility.PowOf2Mod(Length, BitsPerElement);
                if (rem != 0)
                {
                    return (_data![^1] & ~(uint.MaxValue << rem)) == 0;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets or sets a bit at the specified index.
        /// </summary>
        [IndexerName("Bits")]
        public bool this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        /// <summary>
        /// Constructs the bit array with a certain capacity.
        /// </summary>
        public NetBitArray(int bitCapacity)
        {
            if (bitCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(bitCapacity));

            Length = bitCapacity;
            _data = bitCapacity == 0 ? null : new uint[(Length + BitsPerElement - 1) / BitsPerElement];
        }

        [CLSCompliant(false)]
        public Memory<uint> GetBuffer()
        {
            return _data.AsMemory();
        }

        public NetBitArray Slice(int start, int count)
        {
            NetBitArray array = new NetBitArray(count);
            ReadOnlySpan<byte> srcBytes = MemoryMarshal.AsBytes(_data.AsSpan());
            Span<byte> dstBytes = MemoryMarshal.AsBytes(array._data.AsSpan());
            NetBitWriter.CopyBits(srcBytes, start, count, dstBytes, start);
            return array;
        }

        /// <summary>
        /// Shift all bits one step down, cycling the first bit to the top.
        /// </summary>
        public void RotateDown()
        {
            // TODO: check if it can be optimized with BitOperations

            uint[]? data = _data;
            if (data == null)
                return;

            uint firstBit = data[0] & 1;

            for (int i = 0; i < data.Length - 1; i++)
                data[i] = ((data[i] >> 1) & ~(1 << 31)) | data[i + 1] << 31;

            int lastIndex = Length - 1 - (BitsPerElement * (data.Length - 1));

            // special handling of last int
            ref uint last = ref data[^1];
            last >>= 1;
            last |= firstBit << lastIndex;
        }

        /// <summary>
        /// Gets the first (lowest) bit with a given value.
        /// </summary>
        public int IndexOf(bool value, int startIndex, int count)
        {
            uint[]? data = _data;
            if (data == null)
                return -1;

            int elementOffset = DivByESize(startIndex);

            int bitOffset = NetUtility.PowOf2Mod(startIndex, BitsPerElement);
            if (bitOffset != 0)
            {
                int index = FindBit(value, data[elementOffset++], bitOffset, 32 - bitOffset);
                if (index != -1)
                    return index;

                count -= 31 - bitOffset;
                if (count <= 0)
                    return -1;
            }

            if (value)
            {
                for (int i = elementOffset; i < data.Length; i++)
                {
                    uint x = data[i];
                    if (x != 0)
                    {
                        int index = FindBit(value, x, 0, count);
                        if (index == -1)
                            return -1;
                        return bitOffset + i * BitsPerElement + index;
                    }
                    count -= BitsPerElement;
                }
            }
            else
            {
                for (int i = elementOffset; i < data.Length; i++)
                {
                    uint x = data[i];
                    if (x != uint.MaxValue)
                    {
                        int index = FindBit(value, x, 0, count);
                        if (index == -1)
                            return -1;
                        return bitOffset + i * BitsPerElement + index;
                    }
                    count -= BitsPerElement;
                }
            }

            return -1;
        }

        private static int FindBit(bool value, uint buffer, int offset, int count)
        {
            int flag = value ? 1 : 0;
            for (int i = 0; i < count; i++)
            {
                if (((buffer >> (i + offset)) & 1) == flag)
                    return i + offset;
            }
            return -1;
        }

        /// <summary>
        /// Gets the first (lowest) bit with a given value.
        /// </summary>
        public int IndexOf(bool value, int offset)
        {
            return IndexOf(value, offset, Length - offset);
        }

        /// <summary>
        /// Gets the first (lowest) bit with a given value.
        /// </summary>
        public int IndexOf(bool value)
        {
            return IndexOf(value, 0, Length);
        }

        /// <summary>
        /// Gets the bit at the specified index.
        /// </summary>
        public bool Get(int bitIndex)
        {
            uint value = _data![DivByESize(bitIndex)] & (1u << bitIndex);
            return value != 0;
        }

        /// <summary>
        /// Sets or clears the bit at the specified index.
        /// </summary>
        public void Set(int bitIndex, bool value)
        {
            int index = DivByESize(bitIndex);
            uint mask = 1u << NetUtility.PowOf2Mod(bitIndex, BitsPerElement);
            if (value)
            {
                _data![index] |= mask;
            }
            else
            {
                _data![index] &= ~mask;
            }
        }

        /// <summary>
        /// Sets all values to a specified value.
        /// </summary>
        public void SetAll(bool value)
        {
            if (value)
            {
                _data.AsSpan().Fill(uint.MaxValue);
            }
            else
            {
                _data.AsSpan().Clear();
            }
        }

        /// <summary>
        /// Sets all bits to zero.
        /// </summary>
        public void Clear()
        {
            SetAll(false);
        }

        /// <summary>
        /// Returns a string that represents this bit array.
        /// </summary>
        public override string ToString()
        {
            var bdr = new StringBuilder(Length + 2);
            bdr.Append('[');
            for (int i = 0; i < Length; i++)
                bdr.Append(Get(Length - i - 1) ? '1' : '0');
            bdr.Append(']');
            return bdr.ToString();
        }

        public bool Equals(NetBitArray other)
        {
            if (Length != other.Length)
                return false;

            Span<uint> d1 = _data;
            Span<uint> d2 = other._data;

            Span<uint> slice1 = d1.Slice(0, DivByESize(Length));
            Span<uint> slice2 = d2.Slice(0, slice1.Length);

            for (int i = 0; i < slice1.Length; i++)
            {
                if (slice1[i] != slice2[i])
                    return false;
            }

            int rem = NetUtility.PowOf2Mod(Length, BitsPerElement);
            if (rem != 0)
            {
                uint mask = ~(uint.MaxValue << rem);
                return (d1[^1] & mask) == (d2[^1] & mask);
            }
            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is NetBitArray other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                uint result = 17;

                Span<uint> slice = _data.AsSpan(0, DivByESize(Length));
                for (int i = 0; i < slice.Length; i++)
                    result = result * 31 + slice[i];

                int rem = NetUtility.PowOf2Mod(Length, BitsPerElement);
                if (rem != 0)
                {
                    result = result * 31 + (_data![^1] & ~(uint.MaxValue << rem));
                }
                return (int)result;
            }
        }

        public static bool operator ==(NetBitArray left, NetBitArray right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NetBitArray left, NetBitArray right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int DivByESize(int value)
        {
            return value >> 5;
        }
    }
}
