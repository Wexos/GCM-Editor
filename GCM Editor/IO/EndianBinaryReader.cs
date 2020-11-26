using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Editor.IO
{
    /// <summary>
    /// Represents a binary read which reads data from a <see cref="Stream"/>.
    /// </summary>
    public unsafe class EndianBinaryReader : EndianBinaryBase
    {
        private byte[] Buffer;
        private const int DefaultBufferSize = sizeof(ulong);

        /// <summary>
        /// Creates a new <see cref="EndianBinaryReader2"/> object.
        /// </summary>
        /// <param name="Stream">The stream to read the data from.</param>
        /// <param name="Endian">The endianness to read the data as.</param>
        public EndianBinaryReader(Stream Stream, Endianness Endian) : base(Stream, Endian)
        {
            Buffer = new byte[DefaultBufferSize];
        }

        /// <summary>
        /// Fills the internal buffer with data from the stream.
        /// </summary>
        /// <param name="Size">The amount of bytes to read from the stream.</param>
        /// <param name="Stride">The stride in bytes, which is used to reverse bytes caused by endianness.</param>
        private void FillBuffer(int Size, int Stride)
        {
            if (Buffer.Length < Size)
            {
                Buffer = new byte[Size];
            }

            int ReadSize = Stream.Read(Buffer, 0, Size);

            // Check wrong endian
            if (Stride > 1 && EndianReverse)
            {
                fixed (byte* BufferPtr = Buffer)
                {
                    ReverseBytes(BufferPtr, Size, Stride);
                }
            }
        }

        /// <summary>
        /// Returns an unmanaged struct, which is read from the stream. When endianness is not the same as the OS endianness, only structs which can be seen as arrays are supported.
        /// </summary>
        /// <typeparam name="T">The type of struct.</typeparam>
        /// <returns>The structure which was read.</returns>
        public T ReadStruct<T>() where T : unmanaged
        {
            return ReadStruct<T>(sizeof(T));
        }

        /// <summary>
        /// Returns an unmanaged struct, which is read from the stream. When endianness is not the same as the OS endianness, only structs which can be seen as arrays are supported.
        /// </summary>
        /// <typeparam name="T">The type of struct.</typeparam>
        /// <param name="InternalStride">The stride which is used when reversing bytes for endianness.</param>
        /// <returns>The structure which was read.</returns>
        public T ReadStruct<T>(int InternalStride) where T : unmanaged
        {
            int Size = sizeof(T);
            FillBuffer(Size, InternalStride);

            fixed (byte* BufferPtr = Buffer)
            {
                return *(T*)BufferPtr;
            }
        }

        /// <summary>
        /// Returns an array of unmanaged structs, which is read from the stream. When endianness is not the same as the OS endianness, only structs which can be seen as arrays are supported.
        /// </summary>
        /// <typeparam name="T">The type of struct.</typeparam>
        /// <returns>The structure array which was read.</returns>
        public T[] ReadStructArray<T>(int Count) where T : unmanaged
        {
            return ReadStructArray<T>(Count, sizeof(T));
        }

        /// <summary>
        /// Returns an array of unmanaged structs, which is read from the stream. When endianness is not the same as the OS endianness, only structs which can be seen as arrays are supported.
        /// </summary>
        /// <typeparam name="T">The type of struct.</typeparam>
        /// <param name="InternalStride">The stride which is used when reversing bytes for endianness.</param>
        /// <returns>The structure array which was read.</returns>
        public T[] ReadStructArray<T>(int Count, int InternalStride) where T : unmanaged
        {
            int Stride = sizeof(T);
            int Size = Stride * Count;
            FillBuffer(Size, InternalStride);

            T[] Result = new T[Count];

            fixed (byte* BufferPtr = Buffer)
            {
                // This should be faster than copying all individual bytes
                int Offset = 0;

                for (int i = 0; i < Count; i++)
                {
                    Result[i] = *(T*)(BufferPtr + Offset);
                    Offset += Stride;
                }
            }

            return Result;
        }

        /// <summary>
        /// Reads a <see cref="byte"/> from the stream.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public byte ReadByte() => ReadStruct<byte>();

        /// <summary>
        /// Reads a <see cref="sbyte"/> from the stream.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public sbyte ReadSByte() => ReadStruct<sbyte>();

        /// <summary>
        /// Reads a <see cref="ushort"/> from the stream.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public ushort ReadUInt16() => ReadStruct<ushort>();

        /// <summary>
        /// Reads a <see cref="short"/> from the stream.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public short ReadInt16() => ReadStruct<short>();

        public uint ReadUInt24()
        {
            if (Endianness == Endianness.BigEndian)
            {
                return (uint)((ReadByte() << 16) | (ReadByte() << 8) | ReadByte());
            }
            else
            {
                return (uint)(ReadByte() | (ReadByte() << 8) | (ReadByte() << 16));
            }
        }

        /// <summary>
        /// Reads a <see cref="uint"/> from the stream.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public uint ReadUInt32() => ReadStruct<uint>();

        /// <summary>
        /// Reads a <see cref="int"/> from the stream.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public int ReadInt32() => ReadStruct<int>();

        /// <summary>
        /// Reads a <see cref="ulong"/> from the stream.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public ulong ReadUInt64() => ReadStruct<ulong>();

        /// <summary>
        /// Reads a <see cref="int"/> from the stream.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public long ReadInt64() => ReadStruct<long>();

        /// <summary>
        /// Reads a <see cref="float"/> from the stream.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public float ReadSingle() => ReadStruct<float>();

        /// <summary>
        /// Reads a <see cref="double"/> from the stream.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public double ReadDouble() => ReadStruct<double>();

        /// <summary>
        /// Reads a <see cref="decimal"/> from the stream.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public decimal ReadDecimal() => ReadStruct<decimal>();

        /// <summary>
        /// Reads an array of <see cref="byte"/>s from the stream.
        /// </summary>
        /// <param name="Nr">The number of elements which will be read.</param>
        /// <returns>The data that was read.</returns>
        public byte[] ReadBytes(int Nr) => ReadStructArray<byte>(Nr);

        /// <summary>
        /// Reads an array of <see cref="sbyte"/>s from the stream.
        /// </summary>
        /// <param name="Nr">The number of elements which will be read.</param>
        /// <returns>The data that was read.</returns>
        public sbyte[] ReadSBytes(int Nr) => ReadStructArray<sbyte>(Nr);

        /// <summary>
        /// Reads an array of <see cref="ushort"/>s from the stream.
        /// </summary>
        /// <param name="Nr">The number of elements which will be read.</param>
        /// <returns>The data that was read.</returns>
        public ushort[] ReadUInt16s(int Nr) => ReadStructArray<ushort>(Nr);

        /// <summary>
        /// Reads an array of <see cref="short"/>s from the stream.
        /// </summary>
        /// <param name="Nr">The number of elements which will be read.</param>
        /// <returns>The data that was read.</returns>
        public short[] ReadInt16s(int Nr) => ReadStructArray<short>(Nr);

        /// <summary>
        /// Reads an array of <see cref="uint"/>s from the stream.
        /// </summary>
        /// <param name="Nr">The number of elements which will be read.</param>
        /// <returns>The data that was read.</returns>
        public uint[] ReadUInt32s(int Nr) => ReadStructArray<uint>(Nr);

        /// <summary>
        /// Reads an array of <see cref="int"/>s from the stream.
        /// </summary>
        /// <param name="Nr">The number of elements which will be read.</param>
        /// <returns>The data that was read.</returns>
        public int[] ReadInt32s(int Nr) => ReadStructArray<int>(Nr);

        /// <summary>
        /// Reads an array of <see cref="ulong"/>s from the stream.
        /// </summary>
        /// <param name="Nr">The number of elements which will be read.</param>
        /// <returns>The data that was read.</returns>
        public ulong[] ReadUInt64s(int Nr) => ReadStructArray<ulong>(Nr);

        /// <summary>
        /// Reads an array of <see cref="long"/>s from the stream.
        /// </summary>
        /// <param name="Nr">The number of elements which will be read.</param>
        /// <returns>The data that was read.</returns>
        public long[] ReadInt64s(int Nr) => ReadStructArray<long>(Nr);

        /// <summary>
        /// Reads an array of <see cref="float"/>s from the stream.
        /// </summary>
        /// <param name="Nr">The number of elements which will be read.</param>
        /// <returns>The data that was read.</returns>
        public float[] ReadSingles(int Nr) => ReadStructArray<float>(Nr);

        /// <summary>
        /// Reads an array of <see cref="double"/>s from the stream.
        /// </summary>
        /// <param name="Nr">The number of elements which will be read.</param>
        /// <returns>The data that was read.</returns>
        public double[] ReadDoubles(int Nr) => ReadStructArray<double>(Nr);

        /// <summary>
        /// Reads an array of <see cref="decimal"/>s from the stream.
        /// </summary>
        /// <param name="Nr">The number of elements which will be read.</param>
        /// <returns>The data that was read.</returns>
        public decimal[] ReadDecimals(int Nr) => ReadStructArray<decimal>(Nr);

        /// <summary>
        /// Reads an array from the stream.
        /// </summary>
        /// <typeparam name="TValue">The type of value to read from the stream.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="Count">The number of items in the array.</param>
        /// <param name="ConvertionFunction">The function which converts the data type read from the stream to result type.</param>
        /// <returns>The data that was read.</returns>
        public TResult[] ReadArray<TValue, TResult>(int Count, Func<TValue, TResult> ConvertionFunction) where TValue : unmanaged
        {
            TValue[] Array = ReadStructArray<TValue>(Count);
            TResult[] Result = new TResult[Array.Length];

            for (int i = 0; i < Result.Length; i++)
            {
                Result[i] = ConvertionFunction(Array[i]);
            }

            return Result;
        }

        public T[] ReadArray<T>(int Count, Func<EndianBinaryReader, T> ReadFunction)
        {
            T[] Array = new T[Count];

            for (int i = 0; i < Array.Length; i++)
            {
                Array[i] = ReadFunction(this);
            }

            return Array;
        }

        public List<TResult> ReadList<TValue, TResult>(int Count, Func<TValue, TResult> ConvertionFunction) where TValue : unmanaged => new List<TResult>(ReadArray<TValue, TResult>(Count, ConvertionFunction));

        public List<T> ReadList<T>(int Count, Func<EndianBinaryReader, T> ReadFunction) => new List<T>(ReadArray<T>(Count, ReadFunction));

        /// <summary>
        /// Reads a <see cref="string"/> from the stream.
        /// </summary>
        /// <param name="Count">The number of bytes to read.</param>
        /// <param name="Encoding">The text <see cref="Encoding"/> to use.</param>
        /// <returns>The data that was read.</returns>
        public string ReadString(int Count, Encoding Encoding)
        {
            FillBuffer(Count, 1);
            return Encoding.GetString(Buffer, 0, Count);
        }

        /// <summary>
        /// Reads a null-terminated <see cref="string"/> from the stream.
        /// </summary>
        /// <param name="Encoding">The text <see cref="Encoding"/> to use.</param>
        /// <returns>The data that was read.</returns>
        public string ReadStringNT(Encoding Encoding)
        {
            byte[] NullBytes = Encoding.GetBytes("\0");
            List<byte> Read = new List<byte>(0x20);

            // First read as many bytes as NULL consists of
            Read.AddRange(ReadBytes(NullBytes.Length));

            while (true)
            {
                // Check if the last bytes are NULL
                bool IsNull = true;

                for (int i = 0; i < NullBytes.Length; i++)
                {
                    if (Read[Read.Count - (1 + i)] != 0)
                    {
                        IsNull = false;
                        break;
                    }
                }

                if (IsNull)
                {
                    break;
                }

                // Read one more byte
                Read.Add(ReadByte());
            }

            return Encoding.GetString(Read.ToArray(), 0, Read.Count - NullBytes.Length);
        }

        /// <summary>
        /// Reads a <see cref="String"/> with a fixed length from the stream. 00 characters are ignored.
        /// </summary>
        /// <param name="TotalSize">The total size of the <see cref="string"/> in bytes.</param>
        /// <param name="Encoding">The text <see cref="Encoding"/> to use..</param>
        /// <returns>The data that was read.</returns>
        public string ReadFixedString(int TotalLength, Encoding Encoding)
        {
            FillBuffer(TotalLength, 1);

            // TODO: instead, search for the first NULL-byte
            return Encoding.GetString(Buffer, 0, TotalLength).Replace("\0", "");
        }

        /// <summary>
        /// Reads to the end of the stream.
        /// </summary>
        /// <returns>The read byte array.</returns>
        public byte[] ReadToEnd()
        {
            return ReadBytes((int)(StreamLength - Position));
        }

        /// <summary>
        /// Reads a 32-bit Variable-Length Quantity value, represented as a <see cref="Int32"/>.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public int ReadVLQ()
        {
            byte Temp1 = ReadByte();

            if (((Temp1 >> 7) & 1) == 0)
            {
                return Temp1 & 0x7F;
            }

            byte Temp2 = ReadByte();

            if (((Temp2 >> 7) & 1) == 0)
            {
                return ((Temp1 & 0x7F) << 7) | (Temp2 & 0x7F);
            }

            byte Temp3 = ReadByte();

            if (((Temp3 >> 7) & 1) == 0)
            {
                return ((Temp1 & 0x7F) << 14) | ((Temp2 & 0x7F) << 7) | (Temp3 & 0x7F);
            }

            byte Temp4 = ReadByte();

            if (((Temp4 >> 7) & 1) == 0)
            {
                return ((Temp1 & 0x7F) << 21) | ((Temp2 & 0x7F) << 14) | ((Temp3 & 0x7F) << 7) | (Temp4 & 0x7F);
            }

            throw new Exception($"Error at {Position.ToString("X4")}. Invalid VLQ.");
        }

        /// <summary>
        /// Reads a 32-bit usigned <see cref="uint"/>, representing a self-refering offset.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public uint ReadSROffset32U()
        {
            uint Data = ReadUInt32();

            if (Data != 0)
            {
                Data += (uint)Position - 4;
            }

            return Data;
        }

        /// <summary>
        /// Reads a 32-bit signed <see cref="int"/>, representing a self-refering offset.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public int ReadSROffset32()
        {
            int Data = ReadInt32();

            if (Data != 0)
            {
                Data += (int)Position - 4;
            }

            return Data;
        }

        /// <summary>
        /// Reads a 64-bit signed <see cref="Int64"/>, representing a self-refering offset.
        /// </summary>
        /// <returns>The data that was read.</returns>
        public long ReadSROffset64()
        {
            long Data = ReadInt64();

            if (Data != 0)
            {
                Data += Position - 8;
            }

            return Data;
        }

        public string ReadSizedString16(Encoding En)
        {
            return ReadString(ReadUInt16(), En);
        }

        public string ReadSizedString32(Encoding En)
        {
            return ReadString(ReadInt32(), En);
        }

        /// <summary>
        /// Reads a 16-bit Byte-Order-Mark (BOM) from the <see cref="Stream"/> and corrects the <see cref="Endianness"/>.
        /// </summary>
        /// <returns>The data that was read</returns>
        /// <exception cref="BOMException">Thrown when an invalid BOM is read.</exception>
        public ushort ReadBOM()
        {
            UInt16 Data = ReadUInt16();

            if (Data == 0xFEFF)
            {
                return 0xFEFF;
            }
            else if (Data == 0xFFFE)
            {
                SwitchEndianness();
                return 0xFEFF;
            }
            else
            {
                throw new Exception("Invalid BOM");
            }
        }
    }
}
