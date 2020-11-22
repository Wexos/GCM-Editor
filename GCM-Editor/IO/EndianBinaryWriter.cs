using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Editor.IO
{
    /// <summary>
    /// Represents a binary writer which writes data to a <see cref="Stream"/>.
    /// </summary>
    public unsafe class EndianBinaryWriter : EndianBinaryBase
    {
        private byte[] Buffer;
        private const int DefaultBufferSize = sizeof(ulong);

        /// <summary>
        /// Creates a new <see cref="EndianBinaryWriter"/> object.
        /// </summary>
        /// <param name="Stream">The stream to write the data to. The object will not take close or dispose the stream.</param>
        /// <param name="Endian">The endianness to write the data as.</param>
        public EndianBinaryWriter(Stream Stream, Endianness Endian) : base(Stream, Endian)
        {
            Buffer = new byte[DefaultBufferSize];
        }

        /// <summary>
        /// Aligns the <see cref="Stream"/> position by a specified alignment.
        /// </summary>
        /// <param name="Alignment">The alignment to align by.</param>
        public void Align(int Alignment)
        {
            // DO NOT CHANGE THE BEHAVIOURE OF THIS
            while (Stream.Position % Alignment != 0)
            {
                Stream.Position++;
            }
        }

        /// <summary>
        /// Aligns the <see cref="Stream"/> data by a specified alignment.
        /// </summary>
        /// <param name="Alignment">The alignment to align by.</param>
        public void HardAlign(int Alignment)
        {
            HardAlign(Alignment, 0);
        }

        /// <summary>
        /// Aligns the <see cref="Stream"/> data by a specified alignment.
        /// </summary>
        /// <param name="Alignment">The alignment to align by.</param>
        /// <param name="Value">The value to pad with.</param>
        public void HardAlign(int Alignment, byte Value)
        {
            while (Stream.Position % Alignment != 0)
            {
                WriteByte(Value);
            }
        }

        /// <summary>
        /// Aligns the <see cref="Stream"/> data by a specified alignment.
        /// </summary>
        /// <param name="Alignment">The alignment to align by.</param>
        /// <param name="Value">A string which represents the value to pad. The string is iterated over until the alignment is met.</param>
        public void HardAlign(int Alignment, string Value)
        {
            int CharID = 0;

            while (Stream.Position % Alignment != 0)
            {
                WriteByte((byte)Value[CharID]);
                CharID = (CharID + 1) % Value.Length;
            }
        }

        /// <summary>
        /// Aligns the end of the <see cref="Stream"/> by a specified alignment, to make sure the file end is aligned.
        /// </summary>
        /// <param name="Alignment">The alignment to align by.</param>
        public void AlignEndOfFile(int Alignment)
        {
            if (Stream.Length % Alignment != 0)
            {
                long AlignedLength = Stream.Length;

                while (AlignedLength % Alignment != 0)
                {
                    AlignedLength++;
                }

                Stream.SetLength(AlignedLength);
            }
        }

        /// <summary>
        /// Aligns the end of the <see cref="Stream"/> by a specified alignment, to make sure the file end is aligned.
        /// </summary>
        /// <param name="Alignment">The alignment to align by.</param>
        /// <param name="Value">The value to align with.</param>
        public void AlignEndOfFile(int Alignment, byte Value)
        {
            if (Stream.Length % Alignment != 0)
            {
                long Length = Stream.Length;

                while (Length % Alignment != 0)
                {
                    Length++;
                }

                int Padding = (int)(Length - Stream.Length);
                ResizeBuffer(Padding);

                for (int i = 0; i < Buffer.Length; i++)
                {
                    Buffer[i] = Value;
                }

                WriteBuffer(Padding, 1);
            }
        }

        /// <summary>
        /// Ensures the internal buffer is of a certain size.
        /// </summary>
        /// <param name="Size">The size that is needed. The buffer will be at least this size.</param>
        private void ResizeBuffer(int Size)
        {
            if (Size > Buffer.Length)
            {
                Buffer = new byte[Size];
            }
        }

        /// <summary>
        /// Fills the internal buffer with data.
        /// </summary>
        /// <param name="Pattern">The data which all the bytes in the buffer will be set to.</param>
        private void FillBuffer(byte Pattern)
        {
            for (int i = 0; i < Buffer.Length; i++)
            {
                Buffer[i] = Pattern;
            }
        }

        /// <summary>
        /// Writes the internal buffer to the stream.
        /// </summary>
        /// <param name="Size">The number of bytes in the buffer to write.</param>
        /// <param name="Stride">Stride of the data to write in bytes. This is used to reverse bytes caused by endianness.</param>
        private void WriteBuffer(int Size, int Stride)
        {
            // Check wrong endian
            if (Stride > 1 && EndianReverse)
            {
                fixed (byte* BufferPtr = Buffer)
                {
                    ReverseBytes(BufferPtr, Size, Stride);
                }
            }

            Stream.Write(Buffer, 0, Size);
        }

        /// <summary>
        /// Writes an unmanaged struct to the stream. When endianness is not the same as the OS endianness, only structs which can be seen as arrays are supported.
        /// </summary>
        /// <typeparam name="T">The type of struct.</typeparam>
        /// <param name="Data">The data to write.</param>
        public void WriteStruct<T>(T Data) where T : unmanaged
        {
            WriteStruct<T>(Data, sizeof(T));
        }

        /// <summary>
        /// Writes an unmanaged struct to the stream. When endianness is not the same as the OS endianness, only structs which can be seen as arrays are supported.
        /// </summary>
        /// <typeparam name="T">The type of struct.</typeparam>
        /// <param name="Data">The data to write.</param>
        /// <param name="InternalStride">The stride of the elements inside the struct. This is used when reversing bytes for endianness.</param>
        public void WriteStruct<T>(T Data, int InternalStride) where T : unmanaged
        {
            int Size = sizeof(T);
            ResizeBuffer(Size);

            // Copy data to buffer
            IntPtr ObjectPtr = (IntPtr)(byte*)&Data;
            Marshal.Copy(ObjectPtr, Buffer, 0, Size);

            WriteBuffer(Size, InternalStride);
        }

        /// <summary>
        /// Writes a collection of unmanaged structs to the stream. When endianness is not the same as the OS endianness, only structs which can be seen as arrays are supported.
        /// </summary>
        /// <typeparam name="T">The type of struct.</typeparam>
        /// <param name="Data">The data to write.</param>
        public void WriteStructArray<T>(IList<T> Data) where T : unmanaged
        {
            WriteStructArray<T>(Data, sizeof(T));
        }

        /// <summary>
        /// Writes a collection of unmanaged structs to the stream. When endianness is not the same as the OS endianness, only structs which can be seen as arrays are supported.
        /// </summary>
        /// <typeparam name="T">The type of struct.</typeparam>
        /// <param name="Data">The data to write.</param>
        /// <param name="InternalStride">The stride of the elements inside one struct. This is used when reversing bytes for endianness.</param>
        public void WriteStructArray<T>(IList<T> Data, int InternalStride) where T : unmanaged
        {
            int Stride = sizeof(T);
            int Size = Stride * Data.Count;
            ResizeBuffer(Size);

            if (Size == 0)
            {
                return;
            }

            // Optimize array
            if (Data is T[] Array)
            {
                fixed (T* TPtr = Array)
                {
                    IntPtr ArrayPtr = (IntPtr)(byte*)TPtr;
                    Marshal.Copy(ArrayPtr, Buffer, 0, Size);
                }
            }
            else
            {
                int CurrentAddress = 0;

                for (int i = 0; i < Data.Count; i++)
                {
                    T Item = Data[i];
                    IntPtr ItemPtr = (IntPtr)(byte*)&Item;
                    Marshal.Copy(ItemPtr, Buffer, CurrentAddress, Stride);

                    CurrentAddress += Stride;
                }
            }

            WriteBuffer(Size, InternalStride);
        }

        /// <summary>
        /// Writes a <see cref="byte"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteByte(byte Data) => WriteStruct<byte>(Data);

        /// <summary>
        /// Writes a <see cref="sbyte"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteSByte(sbyte Data) => WriteStruct<sbyte>(Data);

        /// <summary>
        /// Writes a <see cref="ushort"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteUInt16(ushort Data) => WriteStruct<ushort>(Data);

        public void WriteUInt24(uint Data)
        {
            if (Endianness == Endianness.BigEndian)
            {
                WriteByte((byte)((Data >> 16) & 0xFF));
                WriteByte((byte)((Data >> 8) & 0xFF));
                WriteByte((byte)(Data & 0xFF));
            }
            else
            {
                WriteByte((byte)(Data & 0xFF));
                WriteByte((byte)((Data >> 16) & 0xFF));
                WriteByte((byte)((Data >> 16) & 0xFF));
            }
        }

        /// <summary>
        /// Writes a <see cref="short"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteInt16(short Data) => WriteStruct<short>(Data);

        /// <summary>
        /// Writes a <see cref="uint"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteUInt32(uint Data) => WriteStruct<uint>(Data);

        /// <summary>
        /// Writes a <see cref="int"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteInt32(int Data) => WriteStruct<int>(Data);

        /// <summary>
        /// Writes a <see cref="ulong"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteUInt64(ulong Data) => WriteStruct<ulong>(Data);

        /// <summary>
        /// Writes a <see cref="long"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteInt64(long Data) => WriteStruct<long>(Data);

        /// <summary>
        /// Writes a <see cref="float"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteSingle(float Data) => WriteStruct<float>(Data);

        /// <summary>
        /// Writes a <see cref="double"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteDouble(double Data) => WriteStruct<double>(Data);

        /// <summary>
        /// Writes a <see cref="decimal"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteDecimal(decimal Data) => WriteStruct<decimal>(Data);

        /// <summary>
        /// Writes a collection of <see cref="byte"/>s to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteBytes(IList<byte> Data) => WriteStructArray<byte>(Data);

        /// <summary>
        /// Writes a collection of <see cref="sbyte"/>s to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteSBytes(IList<sbyte> Data) => WriteStructArray<sbyte>(Data);

        /// <summary>
        /// Writes a collection of <see cref="ushort"/>s to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteUInt16s(IList<ushort> Data) => WriteStructArray<ushort>(Data);

        /// <summary>
        /// Writes a collection of <see cref="short"/>s to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteInt16s(IList<short> Data) => WriteStructArray<short>(Data);

        /// <summary>
        /// Writes a collection of <see cref="uint"/>s to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteUInt32s(IList<uint> Data) => WriteStructArray<uint>(Data);

        /// <summary>
        /// Writes a collection of <see cref="int"/>s to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteInt32s(IList<int> Data) => WriteStructArray<int>(Data);

        /// <summary>
        /// Writes a collection of <see cref="ulong"/>s to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteUInt64s(IList<ulong> Data) => WriteStructArray<ulong>(Data);

        /// <summary>
        /// Writes a collection of <see cref="long"/>s to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteInt64s(IList<long> Data) => WriteStructArray<long>(Data);

        /// <summary>
        /// Writes a collection of <see cref="float"/>s to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteSingles(IList<float> Data) => WriteStructArray<float>(Data);

        /// <summary>
        /// Writes a collection of <see cref="double"/>s to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteDoubles(IList<double> Data) => WriteStructArray<double>(Data);

        /// <summary>
        /// Writes a collection of <see cref="decimal"/>s to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteDecimals(IList<decimal> Data) => WriteStructArray<decimal>(Data);

        /// <summary>
        /// Writes an array to the stream consisting of a specific pattern.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="Count">The number of elements in the array to write.</param>
        /// <param name="Pattern">The pattern value to write.</param>
        public void WritePattern<T>(int Count, T Pattern) where T : unmanaged
        {
            for (int i = 0; i < Count; i++)
            {
                WriteStruct<T>(Pattern);
            }
        }

        /// <summary>
        /// Writes an array to the stream. The values which are written are given by a callback function which depends on the element index.
        /// </summary>
        /// <typeparam name="T">The type of data to write.</typeparam>
        /// <param name="Count">The number of elements in the array to write.</param>
        /// <param name="Callback">The callback function, which takes the element index as an argument, and returns the value for that index.</param>
        public void WriteIndexBasedArray<T>(int Count, Func<int, T> Callback) where T : unmanaged
        {
            for (int i = 0; i < Count; i++)
            {
                WriteStruct<T>(Callback(i));
            }
        }

        /// <summary>
        /// Writes an arbitrary collection to the stream.
        /// </summary>
        /// <typeparam name="T">The type of element in the collection to write.</typeparam>
        /// <param name="Collection">The collection of elements to write.</param>
        /// <param name="WriteAction">The action which writes an item in the collection.</param>
        public void WriteCollection<T>(IList<T> Collection, Action<EndianBinaryWriter, T> WriteAction)
        {
            for (int i = 0; i < Collection.Count; i++)
            {
                WriteAction(this, Collection[i]);
            }
        }

        /// <summary>
        /// Writes an array to the stream.
        /// </summary>
        /// <typeparam name="TInput">The type of the input array.</typeparam>
        /// <typeparam name="TValue">The type of value to write to the stream.</typeparam>
        /// <param name="Input">The array to write to the stream.</param>
        /// <param name="ConvertionFunction">A function which converts the input type to the type to write to the stream.</param>
        public void WriteArray<TInput, TValue>(IList<TInput> Input, Func<TInput, TValue> ConvertionFunction) where TValue : unmanaged
        {
            for (int i = 0; i < Input.Count; i++)
            {
                WriteStruct<TValue>(ConvertionFunction(Input[i]));
            }
        }

        /// <summary>
        /// Writes a <see cref="String"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        /// <param name="En">The text <see cref="Encoding"/> to use for writing the <see cref="String"/>.</param>
        public void WriteString(string Data, Encoding En)
        {
            Buffer = En.GetBytes(Data);
            WriteBuffer(Buffer.Length, 1);
        }

        /// <summary>
        /// Writes a null-terminated <see cref="String"/> to the stream.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        /// <param name="En">The text <see cref="Encoding"/> to use for writing the <see cref="String"/>.</param>
        public void WriteStringNT(string Data, Encoding En)
        {
            WriteString($"{Data}\0", En);
        }

        /// <summary>
        /// Writes a <see cref="String"/> to the stream, which has a fixed length. Unused characters are set to 00.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        /// <param name="TotalSize">The total size of the <see cref="String"/> in bytes.</param>
        /// <param name="En">The text <see cref="Encoding"/> to use for writing the <see cref="String"/>.</param>
        public void WriteFixedString(string Data, int TotalSize, Encoding En)
        {
            byte[] Temp = En.GetBytes(Data.ToCharArray());

            if (TotalSize < Temp.Length)
            {
                throw new ArgumentException($"Error while writing string in {nameof(WriteFixedString)}. Invalid argument {nameof(TotalSize)}.");
            }

            ResizeBuffer(TotalSize);
            FillBuffer(0);

            Array.Copy(Temp, 0, Buffer, 0, Temp.Length);

            WriteBuffer(TotalSize, 1);
        }

        /// <summary>
        /// Writes a 16-bit Byte-Order-Mark (BOM) to the stream. FE FF for <see cref="Endianness.BigEndian"/> and FF FE for <see cref="Endianness.LittleEndian"/>.
        /// </summary>
        public void WriteBOM()
        {
            WriteUInt16(0xFEFF);
        }

        /// <summary>
        /// Writes a 32-bit variable-length quantity (VLQ) value, represented as an <see cref="int"/>.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteVLQ(int Data)
        {
            if (Data < 0 || Data > 0x7FFFFFF)
            {
                throw new ArgumentException("VLQ value is out of range.");
            }

            // TODO: make this better

            if (Data <= 0x7F)
            {
                ResizeBuffer(1);

                Buffer[0] = (byte)(Data & 0x7F);

                WriteBuffer(1, 1);
            }
            else if (Data <= 0x3FFF)
            {
                ResizeBuffer(2);

                Buffer[0] = (byte)((Data >> 7) & 0x7F);
                Buffer[1] = (byte)((Data & 0x7F) | 0x80);

                WriteBuffer(2, 1);
            }
            else if (Data <= 0x1FFFFF)
            {
                ResizeBuffer(3);

                Buffer[0] = (byte)((Data >> 14) & 0x7F);
                Buffer[1] = (byte)(((Data >> 7) & 0x7F) | 0x80);
                Buffer[2] = (byte)((Data & 0x7F) | 0x80);

                WriteBuffer(3, 1);
            }
            else if (Data <= 0x7FFFFFF)
            {
                ResizeBuffer(4);

                Buffer[0] = (byte)((Data >> 21) & 0x7F);
                Buffer[1] = (byte)(((Data >> 14) & 0x7F) | 0x80);
                Buffer[2] = (byte)(((Data >> 7) & 0x7F) | 0x80);
                Buffer[3] = (byte)((Data & 0x7F) | 0x80);

                WriteBuffer(4, 1);
            }
        }

        /// <summary>
        /// Writes a 32-bit unsigned <see cref="uint"/>, representing a self-refering offset.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteSROffset(uint Data)
        {
            WriteUInt32(Data == 0 ? Data : Data - (uint)Position);
        }

        /// <summary>
        /// Writes a 32-bit signed <see cref="int"/>, representing a self-refering offset.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteSROffset(int Data)
        {
            WriteInt32(Data == 0 ? Data : Data - (int)Position);
        }

        /// <summary>
        /// Writes a 64-bit signed <see cref="long"/>, representing a self-refering offset.
        /// </summary>
        /// <param name="Data">The data to write.</param>
        public void WriteSROffset64(long Data)
        {
            WriteInt64(Data == 0 ? Data : Data - Position);
        }
    }
}