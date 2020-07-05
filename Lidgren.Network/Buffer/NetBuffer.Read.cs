﻿using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Text.Unicode;

namespace Lidgren.Network
{
    // TODO: move most code to NetBuffer.Peek
    //       and call it from here and advance the read pointer if it succeeds

    /// <summary>
    /// Base class for <see cref="NetIncomingMessage"/> and <see cref="NetOutgoingMessage"/>.
    /// </summary>
    public partial class NetBuffer
    {
        // TODO: make into extension with ReadEnum

        public bool HasEnough(int bitCount)
        {
            return BitLength - BitPosition >= bitCount;
        }

        /// <summary>
        /// Tries to read a specified number of bits into the given buffer.
        /// </summary>
        /// <param name="destination">The destination span.</param>
        /// <param name="bitCount">The number of bits to read</param>
        public bool TryReadBits(Span<byte> destination, int bitCount)
        {
            if (!HasEnough(bitCount))
                return false;

            NetBitWriter.CopyBits(Span, BitPosition, bitCount, destination, 0);
            BitPosition += bitCount;
            return true;
        }

        /// <summary>
        /// Tries to read bytes into the given span.
        /// </summary>
        /// <param name="destination">The destination span.</param>
        public bool TryRead(Span<byte> destination)
        {
            if (!this.IsByteAligned())
                return TryReadBits(destination, destination.Length * 8);

            if (!HasEnough(destination.Length))
                return false;

            Span.Slice(BytePosition, destination.Length).CopyTo(destination);
            BitPosition += destination.Length * 8;
            return true;
        }

        /// <summary>
        /// Reads bytes into the given span.
        /// </summary>
        /// <param name="destination">The destination span.</param>
        public void Read(Span<byte> destination)
        {
            if (!TryRead(destination))
                throw new EndOfMessageException();
        }

        /// <summary>
        /// Reads the specified number of bits into the given buffer.
        /// </summary>
        /// <param name="destination">The destination span.</param>
        /// <param name="bitCount">The number of bits to read</param>
        public void ReadBits(Span<byte> destination, int bitCount)
        {
            if (!TryReadBits(destination, bitCount))
                throw new EndOfMessageException();
        }

        /// <summary>
        /// Reads the specified number of bits, between one and <paramref name="maxBitCount"/>, into the given buffer.
        /// </summary>
        /// <param name="destination">The destination span.</param>
        /// <param name="bitCount">The number of bits to read</param>
        /// <param name="maxBitCount">The maximum amount of bits to read.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Bit count is less than one or greater than <paramref name="maxBitCount"/>.
        /// </exception>
        public void ReadBitsChecked(Span<byte> destination, int bitCount, int maxBitCount)
        {
            if (bitCount < 1)
                throw new ArgumentOutOfRangeException(nameof(bitCount));
            if (bitCount > maxBitCount)
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            ReadBits(destination, bitCount);
        }

        #region Bool

        /// <summary>
        /// Reads a 1-bit <see cref="bool"/> value written by <see cref="Write(bool)"/>.
        /// </summary>
        public bool ReadBool()
        {
            if (!HasEnough(1))
                throw new EndOfMessageException();

            byte value = NetBitWriter.ReadByteUnchecked(Span, BitPosition, 1);
            BitPosition += 1;
            return value > 0;
        }

        #endregion

        #region Int8

        /// <summary>
        /// Tries to read a <see cref="byte"/>.
        /// </summary>
        /// <returns>Whether the read succeeded.</returns>
        public bool ReadByte(out byte result)
        {
            if (!HasEnough(8))
            {
                result = default;
                return false;
            }

            result = NetBitWriter.ReadByteUnchecked(Span, BitPosition, 8);
            BitPosition += 8;
            return true;
        }

        /// <summary>
        /// Reads a <see cref="byte"/>.
        /// </summary>
        public byte ReadByte()
        {
            if (!ReadByte(out byte value))
                throw new EndOfMessageException();
            return value;
        }

        /// <summary>
        /// Reads 1 to 8 bits into a <see cref="byte"/>.
        /// </summary>
        public byte ReadByte(int bitCount)
        {
            byte value = PeekByte(bitCount);
            BitPosition += bitCount;
            return value;
        }

        /// <summary>
        /// Reads a <see cref="sbyte"/>.
        /// </summary>
        [CLSCompliant(false)]
        public sbyte ReadSByte()
        {
            sbyte value = PeekSByte();
            BitPosition += 8;
            return value;
        }

        #endregion

        #region Int16

        /// <summary>
        /// Reads a 16-bit <see cref="short"/> written by <see cref="Write(short)"/>.
        /// </summary>
        public short ReadInt16()
        {
            Span<byte> tmp = stackalloc byte[sizeof(short)];
            Read(tmp);
            return BinaryPrimitives.ReadInt16LittleEndian(tmp);
        }

        /// <summary>
        /// Reads a 16-bit <see cref="ushort"/> written by <see cref="Write(ushort)"/>.
        /// </summary>
        [CLSCompliant(false)]
        public ushort ReadUInt16()
        {
            Span<byte> tmp = stackalloc byte[sizeof(ushort)];
            Read(tmp);
            return BinaryPrimitives.ReadUInt16LittleEndian(tmp);
        }

        #endregion

        #region Int32

        /// <summary>
        /// Reads a 32-bit <see cref="int"/> written by <see cref="Write(int)"/>.
        /// </summary>
        [CLSCompliant(false)]
        public bool ReadInt32(out int result)
        {
            Span<byte> tmp = stackalloc byte[sizeof(int)];
            if (TryRead(tmp))
            {
                result = BinaryPrimitives.ReadInt32LittleEndian(tmp);
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Reads a 32-bit <see cref="int"/> written by <see cref="Write(int)"/>.
        /// </summary>
        public int ReadInt32()
        {
            Span<byte> tmp = stackalloc byte[sizeof(int)];
            Read(tmp);
            return BinaryPrimitives.ReadInt32LittleEndian(tmp);
        }

        /// <summary>
        /// Reads a <see cref="int"/> stored in 1 to 32 bits, written by <see cref="Write(int, int)"/>.
        /// </summary>
        public int ReadInt32(int bitCount)
        {
            Span<byte> tmp = stackalloc byte[sizeof(int)];
            if (bitCount == tmp.Length * 8)
            {
                Read(tmp);
                return BinaryPrimitives.ReadInt32LittleEndian(tmp);
            }

            ReadBitsChecked(tmp, bitCount, tmp.Length * 8);
            int value = BinaryPrimitives.ReadInt32LittleEndian(tmp);

            int signBit = 1 << (bitCount - 1);
            if ((value & signBit) == 0)
                return value; // positive

            // negative
            unchecked
            {
                uint mask = ((uint)-1) >> (33 - bitCount);
                uint nValue = ((uint)value & mask) + 1;
                return -(int)nValue;
            }
        }

        /// <summary>
        /// Reads a <see cref="uint"/> written by <see cref="Write(uint)"/>.
        /// </summary>
        [CLSCompliant(false)]
        public uint ReadUInt32()
        {
            Span<byte> tmp = stackalloc byte[sizeof(uint)];
            Read(tmp);
            return BinaryPrimitives.ReadUInt32LittleEndian(tmp);
        }

        /// <summary>
        /// Reads a 32-bit <see cref="uint"/> written by <see cref="Write(uint)"/> and returns whether the read succeeded.
        /// </summary>
        [CLSCompliant(false)]
        public bool ReadUInt32(out uint result)
        {
            Span<byte> tmp = stackalloc byte[sizeof(uint)];
            if (TryRead(tmp))
            {
                result = BinaryPrimitives.ReadUInt32LittleEndian(tmp);
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Reads an <see cref="uint"/> stored in 1 to 32 bits, written by <see cref="Write(uint, int)"/>.
        /// </summary>
        [CLSCompliant(false)]
        public uint ReadUInt32(int bitCount)
        {
            Span<byte> tmp = stackalloc byte[sizeof(uint)];
            ReadBitsChecked(tmp, bitCount, tmp.Length * 8);
            return BinaryPrimitives.ReadUInt32LittleEndian(tmp);
        }

        /// <summary>
        /// Reads a 32-bit <see cref="int"/> written by <see cref="WriteRanged"/>.
        /// </summary>
        /// <param name="min">The minimum value used when writing the value</param>
        /// <param name="max">The maximum value used when writing the value</param>
        /// <returns>A signed integer value larger or equal to MIN and smaller or equal to MAX</returns>
        public int ReadRangedInt32(int min, int max)
        {
            uint range = (uint)(max - min);
            int numBits = NetBitWriter.BitCountForValue(range);
            uint rvalue = ReadUInt32(numBits);
            return (int)(min + rvalue);
        }

        #endregion

        #region Int64

        /// <summary>
        /// Reads a 64-bit <see cref="long"/> written by <see cref="Write(long)"/>.
        /// </summary>
        public long ReadInt64()
        {
            Span<byte> tmp = stackalloc byte[sizeof(long)];
            Read(tmp);
            return BinaryPrimitives.ReadInt64LittleEndian(tmp);
        }

        /// <summary>
        /// Reads a 64-bit <see cref="ulong"/> written by <see cref="Write(ulong)"/>.
        /// </summary>
        [CLSCompliant(false)]
        public ulong ReadUInt64()
        {
            Span<byte> tmp = stackalloc byte[sizeof(ulong)];
            Read(tmp);
            return BinaryPrimitives.ReadUInt64LittleEndian(tmp);
        }

        /// <summary>
        /// Reads a <see cref="long"/> stored in 1 to 64 bits, written by <see cref="Write(long, int)"/>.
        /// </summary>
        public long ReadInt64(int bitCount)
        {
            Span<byte> tmp = stackalloc byte[sizeof(long)];
            if (bitCount == tmp.Length * 8)
            {
                Read(tmp);
                return BinaryPrimitives.ReadInt64LittleEndian(tmp);
            }

            ReadBitsChecked(tmp, bitCount, tmp.Length * 8);
            long value = BinaryPrimitives.ReadInt64LittleEndian(tmp);

            long signBit = 1 << (bitCount - 1);
            if ((value & signBit) == 0)
                return value; // positive

            // negative
            unchecked
            {
                ulong mask = ((ulong)-1) >> (65 - bitCount);
                ulong nValue = ((ulong)value & mask) + 1;
                return -(long)nValue;
            }
        }

        /// <summary>
        /// Reads an <see cref="ulong"/> stored in 1 to 64 bits, written by <see cref="Write(ulong, int)"/>.
        /// </summary>
        [CLSCompliant(false)]
        public ulong ReadUInt64(int bitCount)
        {
            Span<byte> tmp = stackalloc byte[sizeof(ulong)];
            ReadBitsChecked(tmp, bitCount, tmp.Length * 8);
            return BinaryPrimitives.ReadUInt64LittleEndian(tmp);
        }

        #endregion

        #region VarInt

        /// <summary>
        /// Tries to read a variable sized <see cref="uint"/> without advancing the read position.
        /// </summary>
        [CLSCompliant(false)]
        public OperationStatus ReadVarUInt32(out uint result)
        {
            return NetBitWriter.ReadVarUInt32(this, peek: false, out result);
        }

        /// <summary>
        /// Tries to read a variable sized <see cref="ulong"/> without advancing the read position.
        /// </summary>
        [CLSCompliant(false)]
        public OperationStatus ReadVarUInt64(out ulong result)
        {
            return NetBitWriter.ReadVarUInt64(this, peek: false, out result);
        }

        [CLSCompliant(false)]
        public uint ReadVarUInt32()
        {
            var status = ReadVarUInt32(out uint value);
            if (status == OperationStatus.Done)
                return value;

            if (status == OperationStatus.NeedMoreData)
                throw new EndOfMessageException();

            return default;
        }

        [CLSCompliant(false)]
        public ulong ReadVarUInt64()
        {
            var status = ReadVarUInt64(out ulong value);
            if (status == OperationStatus.Done)
                return value;

            if (status == OperationStatus.NeedMoreData)
                throw new EndOfMessageException();

            return default;
        }

        /// <summary>
        /// Reads a variable sized <see cref="int"/> written by <see cref="WriteVar(int)"/>.
        /// </summary>
        public int ReadVarInt32()
        {
            uint n = ReadVarUInt32();
            return (int)(n >> 1) ^ -(int)(n & 1); // decode zigzag
        }

        /// <summary>
        /// Reads a variable sized <see cref="long"/> written by <see cref="WriteVar(long)"/>.
        /// </summary>
        public long ReadVarInt64()
        {
            ulong n = ReadVarUInt64();
            return (long)(n >> 1) ^ -(long)(n & 1); // decode zigzag
        }

        #endregion

        #region Float

        /// <summary>
        /// Reads a 32-bit <see cref="float"/>.
        /// </summary>
        public float ReadSingle()
        {
            int intValue = ReadInt32();
            return BitConverter.Int32BitsToSingle(intValue);
        }

        /// <summary>
        /// Reads a 64-bit <see cref="double"/>.
        /// </summary>
        public double ReadDouble()
        {
            long intValue = ReadInt64();
            return BitConverter.Int64BitsToDouble(intValue);
        }

        /// <summary>
        /// Reads a 32-bit <see cref="float"/> written by <see cref="WriteSigned"/>.
        /// </summary>
        /// <param name="bitCount">The number of bits used when writing the value</param>
        /// <returns>A floating point value larger or equal to -1 and smaller or equal to 1</returns>
        public float ReadSignedSingle(int bitCount)
        {
            uint encodedVal = ReadUInt32(bitCount);
            int maxVal = (1 << bitCount) - 1;
            return ((encodedVal + 1) / (float)(maxVal + 1) - 0.5f) * 2.0f;
        }

        /// <summary>
        /// Reads a 32-bit <see cref="float"/> written by <see cref="WriteUnit"/>.
        /// </summary>
        /// <param name="bitCount">The number of bits used when writing the value</param>
        /// <returns>A floating point value larger or equal to 0 and smaller or equal to 1</returns>
        public float ReadUnitSingle(int bitCount)
        {
            uint encodedVal = ReadUInt32(bitCount);
            int maxVal = (1 << bitCount) - 1;
            return (encodedVal + 1) / (float)(maxVal + 1);
        }

        /// <summary>
        /// Reads a 32-bit <see cref="float"/> written by <see cref="WriteRanged"/>.
        /// </summary>
        /// <param name="min">The minimum value used when writing the value</param>
        /// <param name="max">The maximum value used when writing the value</param>
        /// <param name="bitCount">The number of bits used when writing the value</param>
        /// <returns>A floating point value larger or equal to MIN and smaller or equal to MAX</returns>
        public float ReadRangedSingle(float min, float max, int bitCount)
        {
            float range = max - min;
            int maxVal = (1 << bitCount) - 1;
            float encodedVal = ReadUInt32(bitCount);
            float unit = encodedVal / maxVal;
            return min + (unit * range);
        }

        #endregion

        #region ReadString

        public bool ReadStringHeader(out NetStringHeader header)
        {
            header = default;

            if (ReadVarUInt32(out uint charCount) != OperationStatus.Done || charCount > int.MaxValue)
                return false;
            if (charCount <= 0)
                return true;

            if (ReadVarUInt32(out uint byteCount) != OperationStatus.Done || byteCount > int.MaxValue)
                return false;
            if (byteCount <= 0)
                return true;

            if (!HasEnough((int)byteCount * 8))
                return false;

            header = new NetStringHeader(charCount, byteCount);
            return true;
        }

        /// <summary>
        /// Reads chars written by <see cref="Write(ReadOnlySpan{char})"/> or <see cref="Write(string)"/>.
        /// This method does not read <see cref="NetStringHeader"/>.
        /// </summary>
        /// <param name="byteCount">The amount of bytes to read..</param>
        /// <param name="destination">The destination for chars.</param>
        /// <param name="bytesRead">The amount of bytes read.</param>
        public OperationStatus Read(int byteCount, Span<char> destination, out int bytesRead, out int charsWritten)
        {
            if (byteCount < 0)
                throw new ArgumentNullException(nameof(byteCount));

            if (this.IsByteAligned())
            {
                var source = Span.Slice(BytePosition, byteCount);
                var status = Utf8.ToUtf16(source, destination, out bytesRead, out charsWritten, false, false);
                BitPosition += bytesRead * 8;
                return status;
            }
            else
            {
                Span<byte> buffer = stackalloc byte[Math.Min(byteCount, 4096)];
                var status = OperationStatus.Done;
                bytesRead = 0;
                charsWritten = 0;

                while (byteCount > 0)
                {
                    var slice = buffer.Slice(0, Math.Min(buffer.Length, byteCount));
                    Peek(slice);

                    var lastStatus = status;
                    status = Utf8.ToUtf16(
                        buffer, destination, out int sBytesRead, out int sCharsWritten, false, false);

                    bytesRead += sBytesRead;
                    byteCount -= sBytesRead;
                    BitPosition += sBytesRead * 8;

                    charsWritten += sCharsWritten;
                    destination = destination.Slice(sCharsWritten);

                    if (status != OperationStatus.Done)
                    {
                        if (status == OperationStatus.NeedMoreData &&
                            lastStatus != OperationStatus.NeedMoreData &&
                            byteCount > 0)
                            continue;

                        break;
                    }
                }

                return status;
            }
        }

        private static void CreateStringCallback(Span<char> destination, (NetBuffer, NetStringHeader) state)
        {
            var (buffer, header) = state;
            if (header.ByteCount == null)
                return;

            buffer.Read(header.ByteCount.Value, destination, out _, out _);
        }

        /// <summary>
        /// Tries to read a <see cref="string"/> written by 
        /// <see cref="Write(ReadOnlySpan{char})"/> or <see cref="Write(string)"/>.
        /// </summary>
        /// <returns>Whether a string was successfully read.</returns>
        public bool ReadString(out string result)
        {
            if (!ReadStringHeader(out var header))
            {
                result = string.Empty;
                return false;
            }

            if (header.ByteCount == null)
            {
                result = string.Empty;
                return true;
            }

            result = string.Create(header.CharCount, (this, header), CreateStringCallback);
            return true;
        }

        /// <summary>
        /// Reads a <see cref="string"/> written by 
        /// <see cref="Write(ReadOnlySpan{char})"/> or <see cref="Write(string)"/>.
        /// </summary>
        public string ReadString()
        {
            if (!ReadString(out string result))
                throw new EndOfMessageException();
            return result;
        }

        #endregion

        /// <summary>
        /// Byte-aligns the read position, 
        /// decreasing work for subsequent reads if the position was not aligned.
        /// </summary>
        public void SkipPadBits()
        {
            BitPosition = NetBitWriter.ByteCountForBits(BitPosition) * 8;
        }

        /// <summary>
        /// Pads the read position with the specified number of bits.
        /// </summary>
        public void SkipBits(int bitCount)
        {
            BitPosition += bitCount;
        }

        #region TODO: turn these into extension methods

        /// <summary>
        /// Reads local time comparable to <see cref="NetTime.Now"/>,
        /// written by <see cref="WriteLocalTime"/> for the given <see cref="NetConnection"/>.
        /// </summary>
        public TimeSpan ReadLocalTime(NetConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var remoteTime = ReadTimeSpan();
            return connection.GetLocalTime(remoteTime);
        }

        /// <summary>
        /// Reads an <see cref="IPAddress"/>.
        /// </summary>
        public IPAddress ReadIPAddress()
        {
            byte length = ReadByte();
            Span<byte> tmp = stackalloc byte[length];
            return new IPAddress(tmp);
        }

        /// <summary>
        /// Reads an <see cref="IPEndPoint"/> description.
        /// </summary>
        public IPEndPoint ReadIPEndPoint()
        {
            var address = ReadIPAddress();
            var port = ReadUInt16();
            return new IPEndPoint(address, port);
        }

        /// <summary>
        /// Reads a <see cref="TimeSpan"/>.
        /// </summary>
        public TimeSpan ReadTimeSpan()
        {
            return new TimeSpan(ReadVarInt64());
        }

        public TEnum ReadEnum<TEnum>()
            where TEnum : Enum
        {
            long value = ReadVarInt64();
            return EnumConverter.Convert<TEnum>(value);
        }

        #endregion
    }
}
